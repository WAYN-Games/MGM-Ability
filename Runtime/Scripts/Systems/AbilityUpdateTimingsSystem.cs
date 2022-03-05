using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{


    public struct CurrentlyCasting : ISystemStateComponentData
    {
        public float castTime;
        public uint abilityGuid;
        public bool IsCasting => !float.NaN.Equals(castTime) ; 
    }
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public class CooldownRestrictionSystem : SystemBase
    {


        protected override void OnUpdate()
        {
            Entities.ForEach((ref AbilityInput abilityInput,in DynamicBuffer<AbilityCooldownBufferElement> cooldownBuffer, 
                in AbilitiesMapIndex indexMap) => {
                    ref BlobMultiHashMap<uint,int> map = ref indexMap.guidToIndex.Value;
                    var bufferIndex = map.GetValuesForKey(abilityInput.AbilityId)[0];
                    if (cooldownBuffer[bufferIndex].CooldownTime > 0)
                        abilityInput.AddRestriction(16);

                }).Run();
        }
    }

    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    [BurstCompile]
    public struct AbilityUpdateStateAndTimingsSystem : ISystemBase
    {

        private EntityQuery _queryCooldown;
        private EntityQuery _queryCasting;
        private EntityQuery _cache;
        public void OnCreate(ref SystemState state)
        {
            _queryCooldown = state.GetEntityQuery(typeof(AbilityCooldownBufferElement));
            _queryCasting = state.GetEntityQuery(typeof(AbilitiesMapIndex), typeof(AbilityCooldownBufferElement), typeof(CurrentlyCasting),typeof(AbilityInput));
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

        [BurstCompile]
        public struct AbilityUpdateCastingJob : IJobChunk
        {
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public AbilityTimingsCache Cache;
            [ReadOnly]  public ComponentTypeHandle<AbilitiesMapIndex> AbilityMapIndexChunk;

            public ComponentTypeHandle<CurrentlyCasting> CurrentlyCastingChunk;
            public ComponentTypeHandle<AbilityInput> AbilityInputChunk;
            public BufferTypeHandle<AbilityCooldownBufferElement> AbilityCooldownBufferChunk;

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
                    // if casting 
                    if (cc.IsCasting)
                    {
                        // Reduce casting time left
                        cc.castTime -= DeltaTime;
                    }



                    // want to start casting while casting ? add restriction
                    if (ai.IsEnabled() && cc.IsCasting)
                    {
                        ai.AddRestriction(8);
                    }
                    // if applicable (enabled and no retriction)
                    if (ai.IsApplicable())
                    {
                        // Start casting
                        cc.abilityGuid = ai.AbilityId;
                        cc.castTime = timmingCacheMap.GetValuesForKey(cc.abilityGuid)[0].Cast;
                    }

                    abilityInputArray[entityIndex] = ai;
                    currentlyCastingArray[entityIndex] = cc;
                }
            }

        }

        [BurstCompile]
        public struct AbilityUpdpdateCooldownJob : IJobChunk
        {
            [ReadOnly] public float DeltaTime;

            public BufferTypeHandle<AbilityCooldownBufferElement> AbilityCooldownBufferChunk;
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

        }

        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
