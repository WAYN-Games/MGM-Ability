using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system will determine if the current target is in range of a ability.
    /// </summary>
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    [BurstCompile]
    public struct AbilityUpdateRangeSystem : ISystem
    {
        #region Private Fields

        private EntityQuery _cache;
        private EntityQuery _query;

        private ComponentTypeHandle<AbilityInput> _abilityInputTypeHandle;

        #endregion Private Fields

        #region Public Methods

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
    {
                        ComponentType.ReadWrite<AbilityInput>()
    }
            });
            _cache = state.GetEntityQuery(ComponentType.ReadOnly(typeof(SquaredRangeCache)));
            state.RequireSingletonForUpdate<SquaredRangeCache>();

            _abilityInputTypeHandle = state.GetComponentTypeHandle<AbilityInput>(false);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            _abilityInputTypeHandle.Update(ref state);

            state.Dependency = new UpdateRangeJob()
            {
                AbilityInputChunk = _abilityInputTypeHandle,
                Cache = _cache.GetSingleton<SquaredRangeCache>(),
                Position = state.GetComponentDataFromEntity<LocalToWorld>(true),
                TargetChunk = state.GetComponentTypeHandle<Target>(true),
                TranslationChunk = state.GetComponentTypeHandle<Translation>(true)
            }.ScheduleParallel(_query, state.Dependency);
        }

        #endregion Public Methods

        #region Public Structs

        [BurstCompile]
        public struct UpdateRangeJob : IJobEntityBatch
        {
            #region Public Fields

            public ComponentTypeHandle<AbilityInput> AbilityInputChunk;
            [ReadOnly] public SquaredRangeCache Cache;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Position;
            [ReadOnly] public ComponentTypeHandle<Target> TargetChunk;
            [ReadOnly] public ComponentTypeHandle<Translation> TranslationChunk;

            #endregion Public Fields

            #region Public Methods

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                NativeArray<Target> targets = batchInChunk.GetNativeArray(TargetChunk);
                NativeArray<Translation> translations = batchInChunk.GetNativeArray(TranslationChunk);
                NativeArray<AbilityInput> inputs = batchInChunk.GetNativeArray(AbilityInputChunk);

                ref BlobMultiHashMap<uint, Range> cacheMap = ref Cache.Cache.Value;
                for (int entityIndex = 0; entityIndex < batchInChunk.Count; ++entityIndex)
                {
                    AbilityInput ai = inputs[entityIndex];
                    // only check range when trying to activate an ability
                    if (ai.IsEnabled())
                    {
                        Entity target = targets[entityIndex].Value;

                        if (!Position.HasComponent(target)) continue;
                        float3 targetPosition = Position[target].Position;
                        float3 casterPosition = translations[entityIndex].Value;

                        float distance = math.distancesq(casterPosition, targetPosition);
                        Range range = cacheMap.GetValuesForKey(ai.AbilityId)[0];
                        ai.AddRestriction((uint)math.select(2, 0, range.Min <= distance && distance <= range.Max));
                        inputs[entityIndex] = ai;
                    }
                }
            }

            #endregion Public Methods
        }

        #endregion Public Structs
    }
}