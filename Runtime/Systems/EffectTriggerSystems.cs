using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{

    /// <summary>
    /// Base system to trigger effects. It provides the shared functionality of checking which skill is active and which target(s) are affected.
    /// </summary>
    /// <typeparam name="EFFECT_BUFFER">The effect buffer that stores all the effect of a EFFECT type with it's corresponding skill index.</typeparam>
    /// <typeparam name="EFFECT">The effect type trigered by this system.</typeparam>
    /// <typeparam name="CONSUMER">The system in charge of consuming the effects once triggered.</typeparam>
    /// <typeparam name="CTX_WRITER">The writer struct in charge of populating the context surroinding the triggered effect like informations about the caster (position, strength,...).</typeparam>
    /// <typeparam name="EFFECT_CTX">The struct containing the effect and it's context like informations about the caster (position, strength,...)</typeparam>
    [UpdateBefore(typeof(SkillDeactivationSystem))]
    public abstract class EffectTriggerSystem<EFFECT_BUFFER, EFFECT, CONSUMER, CTX_WRITER, EFFECT_CTX> : SystemBase
        where EFFECT : struct, IEffect
        where CONSUMER : EffectConsumerSystem<EFFECT, EFFECT_CTX>
        where EFFECT_BUFFER : struct, IEffectBufferElement<EFFECT>
        where CTX_WRITER : struct, IEffectContextWriter<EFFECT>
        where EFFECT_CTX : struct, IEffectContext<EFFECT>
    {
        /// <summary>
        /// The system in charge of consuming the effects once triggered.
        /// </summary>
        private EffectConsumerSystem<EFFECT, EFFECT_CTX> _conusmerSystem;

        /// <summary>
        /// The base query to select entity taht are eligible to this system.
        /// </summary>
        private EntityQuery _query;

        /// <summary>
        /// A method to describe the necessary components on hte caster to populate the effect's context.
        /// </summary>
        /// <returns>EntityQueryDesc</returns>
        protected virtual EntityQueryDesc GetEffectContextEntityQueryDesc()
        {
            return null;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _conusmerSystem = World.GetOrCreateSystem<CONSUMER>();
            EntityQueryDesc baseEntityQueryDesc = new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<SkillBuffer>(),
                        ComponentType.ReadOnly<Target>(),
                        ComponentType.ReadOnly<EFFECT_BUFFER>()
                }
            };
            EntityQueryDesc contextQueryDesc = GetEffectContextEntityQueryDesc();

            _query = contextQueryDesc == null ? GetEntityQuery(baseEntityQueryDesc) : GetEntityQuery(baseEntityQueryDesc, GetEffectContextEntityQueryDesc());

        }

        /// <summary>
        /// Job in charge of the shared logic (targetting, skill activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        [BurstCompile]
        private struct TriggerJob : IJobChunk
        {
            public CTX_WRITER EffectContextWriter;
            [ReadOnly] public ArchetypeChunkBufferType<SkillBuffer> SkillBufferChunk;
            [ReadOnly] public ArchetypeChunkBufferType<EFFECT_BUFFER> EffectBufferChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Target> TargetChunk;
            public NativeStream.Writer ConsumerWriter;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<SkillBuffer> skillBufffers = chunk.GetBufferAccessor(SkillBufferChunk);
                BufferAccessor<EFFECT_BUFFER> effectBuffers = chunk.GetBufferAccessor(EffectBufferChunk);
                NativeArray<Target> targets = chunk.GetNativeArray(TargetChunk);
                EffectContextWriter.PrepareChunk(chunk);

                ConsumerWriter.BeginForEachIndex(chunkIndex);
                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {

                    NativeArray<SkillBuffer> SkillBufferArray = skillBufffers[entityIndex].AsNativeArray();
                    NativeArray<EFFECT_BUFFER> effectBufferArray = effectBuffers[entityIndex].AsNativeArray();
                    for (int skillIndex = 0; skillIndex < SkillBufferArray.Length; skillIndex++)
                    {

                        Skill Skill = SkillBufferArray[skillIndex];
                        if (!Skill.ShouldApplyEffects()) continue;
                        for (int e = 0; e < effectBufferArray.Length; e++)
                        {
                            EFFECT_BUFFER EffectBuffer = effectBufferArray[skillIndex];
                            if (EffectBuffer.SkillIndex != skillIndex) continue;

                            EffectContextWriter.WriteContextualizedEffect(entityIndex, ref ConsumerWriter, EffectBuffer.Effect, targets[entityIndex].Value);

                        }
                    }
                }
                ConsumerWriter.EndForEachIndex();

            }
        }

        /// <summary>
        /// This method delegates the cosntruction of the CTX_WRITER to the derived class.
        /// </summary>
        /// <returns>A struct implementing IEffectContextWriter<EFFECT>.</returns>
        protected abstract CTX_WRITER GetContextWriter();

        protected override void OnUpdate()
        {
            Dependency = new TriggerJob()
            {
                EffectBufferChunk = GetArchetypeChunkBufferType<EFFECT_BUFFER>(true),
                SkillBufferChunk = GetArchetypeChunkBufferType<SkillBuffer>(true),
                TargetChunk = GetArchetypeChunkComponentType<Target>(true),
                ConsumerWriter = _conusmerSystem.GetConsumerWriter(_query.CalculateChunkCount()),
                EffectContextWriter = GetContextWriter()
            }.ScheduleParallel(_query, Dependency);
            _conusmerSystem.RegisterTriggerDependency(Dependency);
        }
    }

}
