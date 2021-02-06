
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
    public struct AbilityUpdateRangeSystem : ISystemBase
    {
        private EntityQuery _query;
        private EntityQuery _cache;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(typeof(AbilityBufferElement));
            _cache = state.GetEntityQuery(ComponentType.ReadOnly(typeof(RangeCache)));
            state.RequireSingletonForUpdate<RangeCache>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            RangeCache cache = _cache.GetSingleton<RangeCache>();
            state.Dependency = new UpdateRangeJob()
            {
                TargetChunk = state.GetComponentTypeHandle<Target>(true),
                EntityChunk = state.GetEntityTypeHandle(),
                Translations = state.GetComponentDataFromEntity<Translation>(true),
                AbilityBufferChunk = state.GetBufferTypeHandle<AbilityBufferElement>(),
                Cache = cache.Cache
            }.ScheduleParallel(_query, state.Dependency);

        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public struct UpdateRangeJob : IJobChunk
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Translations;
            [ReadOnly] public ComponentTypeHandle<Target> TargetChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public BlobAssetReference<BlobMultiHashMap<uint, Range>> Cache;

            public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBufferElement> abilityBuffers = chunk.GetBufferAccessor(AbilityBufferChunk);
                NativeArray<Target> targets = chunk.GetNativeArray(TargetChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

                ref var cacheMap = ref Cache.Value;

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    Entity caster = entities[entityIndex];
                    Entity target = targets[entityIndex].Value;


                    if (!Translations.HasComponent(caster) || !Translations.HasComponent(targets[entityIndex].Value)) return;
                    float3 casterPosition = Translations[caster].Value;
                    float3 targetPosition = Translations[target].Value;

                    NativeArray<AbilityBufferElement> sbArray = abilityBuffers[entityIndex].AsNativeArray();
                    for (int i = 0; i < sbArray.Length; i++)
                    {
                        AbilityBufferElement ability = sbArray[i];
                        float distance = math.distance(casterPosition, targetPosition);
                        Range range = cacheMap.GetValuesForKey(ability.Guid)[0];
                        ability.IsInRange = range.Min <= distance && distance <= range.Max;
                        sbArray[i] = ability;
                    }
                }
            }
        }


    }
}
