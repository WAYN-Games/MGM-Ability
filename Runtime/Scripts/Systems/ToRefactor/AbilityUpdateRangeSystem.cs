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

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(typeof(AbilityBufferElement));

        }
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new UpdateRangeJob()
            {
                TargetChunk = state.GetComponentTypeHandle<Target>(true),
                EntityChunk = state.GetEntityTypeHandle(),
                Translations = state.GetComponentDataFromEntity<Translation>(true),
                AbilityBufferChunk = state.GetBufferTypeHandle<AbilityBufferElement>()
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

            public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBufferElement> abilityBuffers = chunk.GetBufferAccessor(AbilityBufferChunk);
                NativeArray<Target> targets = chunk.GetNativeArray(TargetChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

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
                        // ability.IsInRange = ability.Range.Min <= distance && distance <= ability.Range.Max;
                        sbArray[i] = ability;
                    }
                }
            }
        }


    }
}
