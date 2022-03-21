using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    [UpdateAfter(typeof(AbilityCostsCheckerSystemGroup))]
    [BurstCompile]
    public struct AbilityUpdateStateAndTimingsSystem : ISystem
    {
        #region Private Fields

        private EntityQuery _queryCooldown;
        private EntityQuery _queryCasting;
        private EntityQuery _cache;

        #endregion Private Fields

        #region Public Methods

        public void OnCreate(ref SystemState state)
        {
            _queryCooldown = state.GetEntityQuery(typeof(AbilityCooldownBufferElement));
            _queryCasting = state.GetEntityQuery(typeof(AbilitiesMapIndex), typeof(AbilityCooldownBufferElement), typeof(CurrentlyCasting), typeof(AbilityInput));
            _cache = state.GetEntityQuery(ComponentType.ReadOnly(typeof(AbilityTimingsCache)));
            state.RequireSingletonForUpdate<AbilityTimingsCache>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Cooldown all abilities
            state.Dependency = new AbilityUpdpdateCooldownJob()
            {
                DeltaTime = state.WorldUnmanaged.CurrentTime.DeltaTime,
                AbilityCooldownBufferChunk = state.GetBufferTypeHandle<AbilityCooldownBufferElement>()
            }.ScheduleParallel(_queryCooldown, state.Dependency);

            state.Dependency = new AbilityUpdateCastingJob()
            {
                DeltaTime = state.WorldUnmanaged.CurrentTime.DeltaTime,
                Cache = _cache.GetSingleton<AbilityTimingsCache>(),
                AbilityMapIndexChunk = state.GetComponentTypeHandle<AbilitiesMapIndex>(true),
                CurrentlyCastingChunk = state.GetComponentTypeHandle<CurrentlyCasting>(),
                AbilityInputChunk = state.GetComponentTypeHandle<AbilityInput>(),
                AbilityCooldownBufferChunk = state.GetBufferTypeHandle<AbilityCooldownBufferElement>()
            }.ScheduleParallel(_queryCasting, state.Dependency);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        #endregion Public Methods

        #region Public Structs

        [BurstCompile]
        public struct AbilityUpdateCastingJob : IJobChunk
        {
            #region Public Fields

            [ReadOnly] public float DeltaTime;
            [ReadOnly] public AbilityTimingsCache Cache;
            [ReadOnly] public ComponentTypeHandle<AbilitiesMapIndex> AbilityMapIndexChunk;

            public ComponentTypeHandle<CurrentlyCasting> CurrentlyCastingChunk;
            public ComponentTypeHandle<AbilityInput> AbilityInputChunk;
            public BufferTypeHandle<AbilityCooldownBufferElement> AbilityCooldownBufferChunk;

            #endregion Public Fields

            #region Public Methods

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AbilitiesMapIndex> abilityMapIndexer = chunk.GetNativeArray(AbilityMapIndexChunk);
                BufferAccessor<AbilityCooldownBufferElement> abilityCooldownBufferElementAccessor = chunk.GetBufferAccessor(AbilityCooldownBufferChunk);
                NativeArray<CurrentlyCasting> currentlyCastingArray = chunk.GetNativeArray(CurrentlyCastingChunk);
                NativeArray<AbilityInput> abilityInputArray = chunk.GetNativeArray(AbilityInputChunk);

                ref BlobMultiHashMap<uint, AbilityTimings> timmingCacheMap = ref Cache.Cache.Value;

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    CurrentlyCasting cc = currentlyCastingArray[entityIndex];
                    AbilityInput ai = abilityInputArray[entityIndex];
                    // if casting
                    // want to start casting while casting ? add restriction
                    if (ai.IsEnabled() && cc.IsCasting)
                    {
                        ai.AddRestriction(8);
                    }
                    if (cc.castTime < 0)
                    {
                        // If casting finished, star coolingdown
                        var entityIndexToGuid = abilityMapIndexer[entityIndex];
                        ref BlobMultiHashMap<uint, int> guidToIndex = ref entityIndexToGuid.guidToIndex.Value;
                        int index = guidToIndex.GetValuesForKey(cc.abilityGuid)[0];
                        var timmings = timmingCacheMap.GetValuesForKey(cc.abilityGuid)[0];

                        NativeArray<AbilityCooldownBufferElement> acbe = abilityCooldownBufferElementAccessor[entityIndex].AsNativeArray();
                        AbilityCooldownBufferElement abilityCooldown = acbe[index];
                        abilityCooldown.CooldownTime = timmings.CoolDown;
                        acbe[index] = abilityCooldown;
                        // and stop casting
                        cc.castTime = float.NaN;
                    }
                    if (cc.IsCasting)
                    {
                        // Reduce casting time left
                        cc.castTime -= DeltaTime;
                    }

                    // if applicable (enabled and no retriction)
                    if (ai.IsApplicable() && !cc.IsCasting)
                    {
                        // Start casting
                        cc.abilityGuid = ai.AbilityId;
                        cc.castTime = timmingCacheMap.GetValuesForKey(cc.abilityGuid)[0].Cast;
                    }

                    abilityInputArray[entityIndex] = ai;
                    currentlyCastingArray[entityIndex] = cc;
                }
            }

            #endregion Public Methods
        }

        [BurstCompile]
        public struct AbilityUpdpdateCooldownJob : IJobChunk
        {
            #region Public Fields

            [ReadOnly] public float DeltaTime;

            public BufferTypeHandle<AbilityCooldownBufferElement> AbilityCooldownBufferChunk;

            #endregion Public Fields

            #region Public Methods

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityCooldownBufferElement> abilityBufferAccessor = chunk.GetBufferAccessor(AbilityCooldownBufferChunk);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    NativeArray<AbilityCooldownBufferElement> sbArray = abilityBufferAccessor[entityIndex].AsNativeArray();
                    Cooldown(sbArray, DeltaTime);
                }

                void Cooldown(NativeArray<AbilityCooldownBufferElement> sbArray, float DeltaTime)
                {
                    for (int i = 0; i < sbArray.Length; i++)
                    {
                        var ability = sbArray[i];
                        ability.CooldownTime -= DeltaTime;
                        sbArray[i] = ability;
                    }
                }
            }

            #endregion Public Methods
        }

        #endregion Public Structs
    }
}