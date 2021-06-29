using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{


    public struct CurrentlyCasting : ISystemStateComponentData
    {
        public bool IsCasting;
        public uint abilityId;
        public float castTime;
        public int index;
    }
    /// <summary>
    /// This system update each ability's timming (cast and/or cooldown).
    /// Once a timing is elapsed, the ability state is updated to the next value in the life cycle.
    /// (Casting -> Active -> CoolingDown -> CooledDown -> Casting)
    /// </summary>
    /// 
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    [UpdateAfter(typeof(OldAbilityUpdateStateAndTimingsSystem))]
    [BurstCompile]
    public struct AbilityUpdateStateAndTimingsSystem : ISystemBase
    {
        private EntityQuery _query;
        private EntityQuery _cache;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(typeof(AbilityBufferElement), typeof(AbilityInput), typeof(CurrentlyCasting));
            _cache = state.GetEntityQuery(ComponentType.ReadOnly(typeof(AbilityTimingsCache)));
            state.RequireSingletonForUpdate<AbilityTimingsCache>();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            AbilityTimingsCache cache = _cache.GetSingleton<AbilityTimingsCache>();
            state.Dependency = new AbilityUpdateStateAndTimingsJob()
            {
                DeltaTime = state.WorldUnmanaged.CurrentTime.DeltaTime,
                CurrentlyCastingChunk = state.GetComponentTypeHandle<CurrentlyCasting>(),
                AbilityInputChunk = state.GetComponentTypeHandle<AbilityInput>(),
                AbilityBufferChunk = state.GetBufferTypeHandle<AbilityBufferElement>(),
                Cache = cache.Cache

            }.ScheduleParallel(_query, state.Dependency);
        }


        [BurstCompile]
        public struct AbilityUpdateStateAndTimingsJob : IJobChunk
        {
            [ReadOnly] public BlobAssetReference<BlobMultiHashMap<uint, AbilityTimings>> Cache;
            [ReadOnly] public float DeltaTime;

            public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;
            public ComponentTypeHandle<AbilityInput> AbilityInputChunk;
            public ComponentTypeHandle<CurrentlyCasting> CurrentlyCastingChunk;

            public AbilityBufferElement UpdateState(AbilityBufferElement ability, AbilityInput abilityInput, ref CurrentlyCasting currentlyCasting, AbilityTimings timming, int index)
            {
                if (abilityInput.AbilityId == ability.Guid && abilityInput.Enabled)
                {

                    bool canCast = true;
                    if (currentlyCasting.IsCasting)
                    {
                        canCast = false;
                    }
                    else
                    if (ability.AbilityState != AbilityState.CooledDown)
                    {
                        canCast = false;
                    }
                    else
                    if (!ability.IsInRange)
                    {
                        canCast = false;
                    }
                    else
                    if (!ability.HasEnougthRessource)
                    {
                        canCast = false;
                    }

                    if (canCast)
                    {
                        ability = StartCasting(ability, ref currentlyCasting, timming, index);
                        return ability;
                    }
                }

                bool IsCooldownComplete = ability.AbilityState == AbilityState.CoolingDown && ability.CurrentTimming < 0;
                if (IsCooldownComplete)
                {
                    ability = WaitForActivation(ability);
                    return ability;
                }

                if (ability.AbilityState == AbilityState.Active)
                {
                    ability = StartCoolDown(ability, timming);
                    return ability;
                }

                bool IsCastComplete = ability.AbilityState == AbilityState.Casting && ability.CurrentTimming < 0;

                if (IsCastComplete)
                {
                    ability = Activate(ability, ref currentlyCasting);
                    return ability;
                }

                return ability;
            }

            public AbilityBufferElement WaitForActivation(AbilityBufferElement Ability)
            {
                Ability.AbilityState = AbilityState.CooledDown;
                return Ability;
            }

            public AbilityBufferElement StartCasting(AbilityBufferElement ability, ref CurrentlyCasting casting, AbilityTimings timming, int index)
            {

                ability.AbilityState = AbilityState.Casting;
                ability.CurrentTimming = timming.Cast;
                casting.IsCasting = true;
                casting.castTime = timming.Cast;
                casting.abilityId = ability.Guid;
                casting.index = index;

                return ability;
            }

            public AbilityBufferElement Activate(AbilityBufferElement Ability, ref CurrentlyCasting casting)
            {
                if (!casting.IsCasting) return Ability;
                Ability.AbilityState = AbilityState.Active;
                casting.IsCasting = false;
                return Ability;
            }

            public AbilityBufferElement StartCoolDown(AbilityBufferElement Ability, AbilityTimings timming)
            {
                Ability.AbilityState = AbilityState.CoolingDown;
                Ability.CurrentTimming = timming.CoolDown;
                return Ability;
            }


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBufferElement> abilityBufferAccessor = chunk.GetBufferAccessor(AbilityBufferChunk);
                NativeArray<AbilityInput> inputArray = chunk.GetNativeArray(AbilityInputChunk);
                NativeArray<CurrentlyCasting> currentlyCastingArray = chunk.GetNativeArray(CurrentlyCastingChunk);

                ref var cahcheMap = ref Cache.Value;

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    AbilityInput input = inputArray[entityIndex];
                    CurrentlyCasting currentlyCasting = currentlyCastingArray[entityIndex];
                    NativeArray<AbilityBufferElement> sbArray = abilityBufferAccessor[entityIndex].AsNativeArray();
                    for (int i = 0; i < sbArray.Length; i++)
                    {
                        var ability = sbArray[i];
                        var timmings = cahcheMap.GetValuesForKey(ability.Guid);


                        AbilityTimings timming = timmings[0];
                        ability.CurrentTimming -= DeltaTime;
                        ability = UpdateState(ability, input, ref currentlyCasting, timming, i);

                        sbArray[i] = ability;
                    }
                    input.Enabled = false;
                    inputArray[entityIndex] = input;
                    currentlyCastingArray[entityIndex] = currentlyCasting;
                }
            }
        }


        public void OnDestroy(ref SystemState state)
        {

        }
    }




    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public class OldAbilityUpdateStateAndTimingsSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            Entities.WithNone<CurrentlyCasting>().WithAll<AbilityBufferElement>().WithStructuralChanges().ForEach((Entity entity) =>
            {
                EntityManager.AddComponentData(entity, new CurrentlyCasting() { IsCasting = false });
            }).WithoutBurst().Run();

        }

    }
}
