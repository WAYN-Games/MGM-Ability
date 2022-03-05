
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system will determine if the current target is in range of a ability.
    /// </summary>
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    [BurstCompile]
    public struct AbilityUpdateRangeSystem : ISystemBase
    {
        private EntityQuery _query;
        private EntityQuery _cache;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
    {
                        ComponentType.ReadWrite<AbilityInput>()
    }
            });
            _cache = state.GetEntityQuery(ComponentType.ReadOnly(typeof(RangeCache)));
            state.RequireSingletonForUpdate<RangeCache>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new UpdateRangeJob()
            {
                TargetChunk = state.GetComponentTypeHandle<Target>(true),
                EntityChunk = state.GetEntityTypeHandle(),
                AbilityInputChunk = state.GetComponentTypeHandle<AbilityInput>(false),
                Translations = state.GetComponentDataFromEntity<Translation>(true),
                Cache = _cache.GetSingleton<RangeCache>()
            }.ScheduleParallel(_query, 1, state.Dependency);
        }

        [BurstCompile]
        public struct UpdateRangeJob : IJobEntityBatch
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Translations;
            [ReadOnly] public ComponentTypeHandle<Target> TargetChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public RangeCache Cache;
            public ComponentTypeHandle<AbilityInput> AbilityInputChunk;



            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                NativeArray<Target> targets = batchInChunk.GetNativeArray(TargetChunk);
                NativeArray<AbilityInput> inputs = batchInChunk.GetNativeArray(AbilityInputChunk);
                NativeArray<Entity> entities = batchInChunk.GetNativeArray(EntityChunk);

                ref BlobMultiHashMap<uint, Range> cacheMap = ref Cache.Cache.Value;
                for (int entityIndex = 0; entityIndex < entities.Length; ++entityIndex)
                {
                    AbilityInput ai = inputs[entityIndex];
                    // only check range when trying to activate an ability
                    if (ai.IsEnabled())
                    {
                        Entity caster = entities[entityIndex];
                        Entity target = targets[entityIndex].Value;

                        if (!Translations.HasComponent(caster) || !Translations.HasComponent(target)) continue;
                        float3 casterPosition = Translations[caster].Value;
                        float3 targetPosition = Translations[target].Value;

                        float distance = math.distancesq(casterPosition, targetPosition);
                        Range range = cacheMap.GetValuesForKey(ai.AbilityId)[0];
                        bool inRange = math.pow(range.Min, 2) <= distance && distance <= math.pow(range.Max, 2);
                        if (!inRange)
                        {
                            ai.AddRestriction(2);
                        }
                        inputs[entityIndex] = ai;
                    }
                }
                
            }
        }
    }
}
