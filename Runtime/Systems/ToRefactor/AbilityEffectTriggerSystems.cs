using System;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{


    /// <summary>
    /// Base system to trigger effects. It provides the shared functionality of checking which ability is active and which target(s) are affected.
    /// </summary>
    /// <typeparam name="EFFECT">The effect type trigered by this system.</typeparam>
    /// <typeparam name="CONSUMER">The system in charge of consuming the effects once triggered.</typeparam>
    /// <typeparam name="CTX_BUILDER">The writer struct in charge of populating the context surroinding the triggered effect like informations about the caster (position, strength,...).</typeparam>
    /// <typeparam name="EFFECT_CTX">The struct containing the effect and it's context like informations about the caster (position, strength,...)</typeparam>
    [UpdateInGroup(typeof(AbilityTriggerSystemGroup))]
    public abstract class AbilityEffectTriggerSystem<EFFECT, CONSUMER, CTX_BUILDER, EFFECT_CTX> : SystemBase
        where EFFECT : struct, IEffect
        where CONSUMER : AbilityEffectConsumerSystem<EFFECT, EFFECT_CTX>
        where CTX_BUILDER : struct, IEffectContextWriter<EFFECT_CTX>
        where EFFECT_CTX : struct, IEffectContext
    {
        /// <summary>
        /// The system in charge of consuming the effects once triggered.
        /// </summary>
        private AbilityEffectConsumerSystem<EFFECT, EFFECT_CTX> _conusmerSystem;
        /// <summary>
        /// The base query to select entity that are eligible to this system.
        /// </summary>
        private EntityQuery _query;
        /// <summary>
        /// A map of all effects' unmutable data for the EFFECT type.
        /// </summary>
        private NativeMultiHashMap<Guid, EFFECT> _effectMap;

        protected override void OnCreate()
        {
            base.OnCreate();
            ListenfForEffectCatalogUpdate();
            CacheEffectConsumerSystem();
            BuildEntityQuery();
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            // If the consumer won't run, there is no point in tirgerring the effects...
            // This also avoid the creation of a stream that would never be disposed of.
            if (!_conusmerSystem.ShouldRunSystem()) return;

            Dependency = new TriggerJob()
            {
                AbilityBufferChunk = GetBufferTypeHandle<AbilityBufferElement>(true),
                TargetChunk = GetComponentTypeHandle<Target>(true),
                EntityChunk = GetEntityTypeHandle(),
                ConsumerWriter = _conusmerSystem.CreateConsumerWriter(_query.CalculateChunkCount()),
                EffectContextBuilder = GetContextWriter(),
                EffectMap = _effectMap
            }.ScheduleParallel(_query, Dependency);

            // Tell the consumer to wait for ths trigger job to finish before starting to consume the effects.
            _conusmerSystem.RegisterTriggerDependency(Dependency);
        }

        /// <summary>
        /// Job in charge of the shared logic (targetting, ability activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        private struct TriggerJob : IJobChunk
        {
            public CTX_BUILDER EffectContextBuilder;
            [ReadOnly] public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;
            [ReadOnly] public ComponentTypeHandle<Target> TargetChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public NativeMultiHashMap<Guid, EFFECT> EffectMap;
            public NativeStream.Writer ConsumerWriter;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBufferElement> abilityBufffers = chunk.GetBufferAccessor(AbilityBufferChunk);
                NativeArray<Target> targets = chunk.GetNativeArray(TargetChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                EffectContextBuilder.PrepareChunk(chunk);


                ConsumerWriter.BeginForEachIndex(chunkIndex);
                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    Guid activeAbilityIndex = FindActiveAbility(abilityBufffers, entityIndex, abilityBufffers[entityIndex].AsNativeArray());
                    WriteEffectsForActiveAbility(targets, entities, entityIndex, activeAbilityIndex);
                }
                ConsumerWriter.EndForEachIndex();

            }

            private void WriteEffectsForActiveAbility(NativeArray<Target> targets, NativeArray<Entity> entities, int entityIndex, Guid activeAbilityGuid)
            {
                if (activeAbilityGuid.Equals(default)) return;

                NativeMultiHashMap<Guid, EFFECT>.Enumerator effectEnumerator = EffectMap.GetValuesForKey(activeAbilityGuid);
                while (effectEnumerator.MoveNext())
                {
                    ConsumerWriter.Write(effectEnumerator.Current);
                    NativeArray<Entity> targetEntities = FindTargets(ref targets, ref entities, entityIndex, effectEnumerator.Current.Affects);

                    ConsumerWriter.Write(EffectContextBuilder.BuildEffectContext(entityIndex));
                    ConsumerWriter.Write(targetEntities.Length);
                    for (int i = 0; i < targetEntities.Length; ++i)
                    {
                        ConsumerWriter.Write(targetEntities[i]);
                    }

                }
            }

            private static NativeArray<Entity> FindTargets(ref NativeArray<Target> targets, ref NativeArray<Entity> entities, int entityIndex, TargetingMode efffectTargetingMode)
            {
                NativeArray<Entity> targetEntities = new NativeArray<Entity>(1, Allocator.Temp);
                targetEntities[0] = efffectTargetingMode == TargetingMode.Target ? targets[entityIndex].Value : entities[entityIndex];
                return targetEntities;
            }

            private static Guid FindActiveAbility(BufferAccessor<AbilityBufferElement> abilityBufffers, int entityIndex, NativeArray<AbilityBufferElement> AbilityBufferArray)
            {
                for (int abilityIndex = 0; abilityIndex < AbilityBufferArray.Length; ++abilityIndex)
                {
                    if (AbilityBufferArray[abilityIndex].AbilityState != AbilityState.Active) continue;
                    return AbilityBufferArray[abilityIndex].Guid;
                }
                return default;
            }
        }




        #region Self Documenting Encapsulations

        /// <summary>
        /// This method delegates the cosntruction of the CTX_WRITER to the derived class.
        /// </summary>
        /// <returns>A struct implementing IEffectContextWriter<EFFECT>.</returns>
        protected virtual CTX_BUILDER GetContextWriter()
        {
            return default;
        }

        private void UpdateEffectCache(MultiMap<Type, EffectData> effectMap)
        {
            NativeMultiHashMap<Guid, EFFECT> map = BuildEffectMapCache(effectMap);
            RefreshEffectMapChache(map);
            Enabled = true;
        }

        private void RefreshEffectMapChache(NativeMultiHashMap<Guid, EFFECT> map)
        {
            if (_effectMap.IsCreated) _effectMap.Dispose();
            _effectMap = map;
        }

        private static NativeMultiHashMap<Guid, EFFECT> BuildEffectMapCache(MultiMap<Type, EffectData> effectMap)
        {
            NativeMultiHashMap<Guid, EFFECT> map = new NativeMultiHashMap<Guid, EFFECT>(effectMap.Count(typeof(EFFECT)), Allocator.Persistent);
            foreach (EffectData effectData in effectMap[typeof(EFFECT)])
            {
                map.Add(effectData.guid, (EFFECT)effectData.effect);
            }
            return map;
        }

        private void ListenfForEffectCatalogUpdate()
        {
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnEffectUpdate += UpdateEffectCache;
        }

        private void CacheEffectConsumerSystem()
        {
            _conusmerSystem = World.GetOrCreateSystem<CONSUMER>();
        }

        private void BuildEntityQuery()
        {
            EntityQueryDesc baseEntityQueryDesc = new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<AbilityBufferElement>(),
                        ComponentType.ReadOnly<Target>()
                }
            };

            EntityQueryDesc contextQueryDesc = GetEffectContextEntityQueryDesc();

            EntityQueryDesc entityQueryDesc = new EntityQueryDesc()
            {
                All = baseEntityQueryDesc.All.Concat(contextQueryDesc.All).ToArray(),
                Any = baseEntityQueryDesc.Any.Concat(contextQueryDesc.Any).ToArray(),
                None = baseEntityQueryDesc.None.Concat(contextQueryDesc.None).ToArray(),
                Options = contextQueryDesc.Options
            };


            _query = GetEntityQuery(entityQueryDesc);
        }

        /// <summary>
        /// A method to describe the necessary components on the caster to populate the effect's context.
        /// </summary>
        /// <returns>EntityQueryDesc</returns>
        protected virtual EntityQueryDesc GetEffectContextEntityQueryDesc()
        {
            return new EntityQueryDesc();
        }
        #endregion
    }

}
