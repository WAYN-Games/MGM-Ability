
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
    public class AbilityUpdateRangeSystem : SystemBase
    {
        private EntityQuery _query;
        private EntityQuery _cache;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadWrite<AbilityBufferElement>(),
                        ComponentType.ReadOnly<AbilityInput>()
                }
            });
            _cache = GetEntityQuery(ComponentType.ReadOnly(typeof(RangeCache)));
            RequireSingletonForUpdate<RangeCache>();
        }



        protected override void OnUpdate()
        {
            RangeCache cache = _cache.GetSingleton<RangeCache>();
            Dependency = new UpdateRangeJob()
            {
                TargetChunk = GetComponentTypeHandle<Target>(true),
                EntityChunk = GetEntityTypeHandle(),
                AbilityInputChunk = GetComponentTypeHandle<AbilityInput>(true),
                Translations = GetComponentDataFromEntity<Translation>(true),
                AbilityBufferChunk = GetBufferTypeHandle<AbilityBufferElement>(),
                Cache = cache.Cache
            }.ScheduleParallel(_query, 1, Dependency);
        }

        [BurstCompile]
        public struct UpdateRangeJob : IJobEntityBatch
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> Translations;
            [ReadOnly] public ComponentTypeHandle<Target> TargetChunk;
            [ReadOnly] public ComponentTypeHandle<AbilityInput> AbilityInputChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public BlobAssetReference<BlobMultiHashMap<uint, Range>> Cache;

            public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;


            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                BufferAccessor<AbilityBufferElement> abilityBuffers = batchInChunk.GetBufferAccessor(AbilityBufferChunk);
                NativeArray<Target> targets = batchInChunk.GetNativeArray(TargetChunk);
                NativeArray<AbilityInput> inputs = batchInChunk.GetNativeArray(AbilityInputChunk);
                NativeArray<Entity> entities = batchInChunk.GetNativeArray(EntityChunk);

                ref BlobMultiHashMap<uint, Range> cacheMap = ref Cache.Value;

                for (int entityIndex = 0; entityIndex < entities.Length; ++entityIndex)
                {
                    // only check range when trying to activate an ability
                    if (!inputs[entityIndex].Enabled) continue;

                    Entity caster = entities[entityIndex];
                    Entity target = targets[entityIndex].Value;


                    if (!Translations.HasComponent(caster) || !Translations.HasComponent(target)) return;
                    float3 casterPosition = Translations[caster].Value;
                    float3 targetPosition = Translations[target].Value;

                    NativeArray<AbilityBufferElement> sbArray = abilityBuffers[entityIndex].AsNativeArray();
                    for (int i = 0; i < sbArray.Length; i++)
                    {

                        AbilityBufferElement ability = sbArray[i];
                        // only check range for ability to activate an ability
                        if (inputs[entityIndex].AbilityId != ability.Guid) continue;
                        float distance = math.distancesq(casterPosition, targetPosition);
                        NativeArray<Range> ranges = cacheMap.GetValuesForKey(ability.Guid);
                        Range range = ranges[0];
                        ability.IsInRange = math.pow(range.Min, 2) <= distance && distance <= math.pow(range.Max, 2);
                        sbArray[i] = ability;
                    }
                    sbArray.Dispose();
                }
            }
        }
    }
}
