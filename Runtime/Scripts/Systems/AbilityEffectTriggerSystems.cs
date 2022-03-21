using System;
using System.Linq;
using Unity.Burst;
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
    public abstract partial class AbilityEffectTriggerSystem<EFFECT, CONSUMER, CTX_BUILDER, EFFECT_CTX> : SystemBase
        where EFFECT : struct, IEffect
        where CONSUMER : AbilityEffectConsumerSystem<EFFECT, EFFECT_CTX>
        where CTX_BUILDER : struct, IEffectContextWriter<EFFECT_CTX>
        where EFFECT_CTX : struct, IEffectContext
    {
        #region Private Fields

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
        private NativeMultiHashMap<uint, EFFECT> _effectMap;

        #endregion Private Fields

        #region Protected Methods

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
                CurrentlyCastingChunk = GetComponentTypeHandle<CurrentlyCasting>(true),
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
        /// This method delegates the cosntruction of the CTX_WRITER to the derived class.
        /// </summary>
        /// <returns>A struct implementing IEffectContextWriter<EFFECT>.</returns>
        protected virtual CTX_BUILDER GetContextWriter()
        {
            return default;
        }

        /// <summary>
        /// A method to describe the necessary components on the caster to populate the effect's context.
        /// </summary>
        /// <returns>EntityQueryDesc</returns>
        protected virtual EntityQueryDesc GetEffectContextEntityQueryDesc()
        {
            return new EntityQueryDesc();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_effectMap.IsCreated) _effectMap.Dispose();
        }

        #endregion Protected Methods

        #region Private Methods

        private static NativeMultiHashMap<uint, EFFECT> BuildEffectMapCache(MultiHashMap<Type, EffectData> effectMap)
        {
            NativeMultiHashMap<uint, EFFECT> map = new NativeMultiHashMap<uint, EFFECT>(effectMap.Count(typeof(EFFECT)), Allocator.Persistent);
            foreach (EffectData effectData in effectMap[typeof(EFFECT)])
            {
                map.Add(effectData.Guid, (EFFECT)effectData.effect);
            }
            return map;
        }

        private void UpdateEffectCache(MultiHashMap<Type, EffectData> effectMap)
        {
            if (!effectMap.ContainsKey(typeof(EFFECT)))
            {
                _conusmerSystem.Enabled = false;
                return;
            }
            NativeMultiHashMap<uint, EFFECT> map = BuildEffectMapCache(effectMap);
            RefreshEffectMapChache(map);
            Enabled = true;
            _conusmerSystem.Enabled = true;
        }

        private void RefreshEffectMapChache(NativeMultiHashMap<uint, EFFECT> map)
        {
            if (_effectMap.IsCreated) _effectMap.Dispose();
            _effectMap = map;
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
                        ComponentType.ReadOnly<CurrentlyCasting>(),
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

        #endregion Private Methods

        #region Public Structs

        /// <summary>
        /// Job in charge of the shared logic (targetting, ability activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        [BurstCompile]
        public struct TriggerJob : IJobChunk
        {
            #region Public Fields

            [ReadOnly] public ComponentTypeHandle<CurrentlyCasting> CurrentlyCastingChunk;
            [ReadOnly] public ComponentTypeHandle<Target> TargetChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public NativeMultiHashMap<uint, EFFECT> EffectMap;
            public NativeStream.Writer ConsumerWriter;
            public CTX_BUILDER EffectContextBuilder;

            #endregion Public Fields

            #region Public Methods

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<CurrentlyCasting> CurrentlyCastingComponent = chunk.GetNativeArray(CurrentlyCastingChunk);
                NativeArray<Target> targets = chunk.GetNativeArray(TargetChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                EffectContextBuilder.PrepareChunk(chunk);

                // removing this result in exception for more than 2 chunks whatever the index passed to BeginForEachIndex
                ConsumerWriter.PatchMinMaxRange(chunkIndex);

                ConsumerWriter.BeginForEachIndex(chunkIndex);
                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    CurrentlyCasting cc = CurrentlyCastingComponent[entityIndex];
                    NativeMultiHashMap<uint, EFFECT>.Enumerator effectEnumerator = EffectMap.GetValuesForKey(cc.abilityGuid);
                    while (effectEnumerator.MoveNext())
                    {
                        EFFECT effect = effectEnumerator.Current;
                        if (cc.castTime < 0)
                        {
                            ConsumerWriter.Write(effect);
                            Entity targetEntity = FindTargets(ref targets, ref entities, entityIndex, effect.Affects);
                            ConsumerWriter.Write(EffectContextBuilder.BuildEffectContext(entityIndex));
                            ConsumerWriter.Write(1);
                            ConsumerWriter.Write(targetEntity);
                        }
                    }
                    effectEnumerator.Dispose();
                }
                ConsumerWriter.EndForEachIndex();
            }

            #endregion Public Methods

            #region Private Methods

            private static Entity FindTargets(ref NativeArray<Target> targets, ref NativeArray<Entity> entities, int entityIndex, TargetingMode efffectTargetingMode)
            {
                return efffectTargetingMode == TargetingMode.Target ? targets[entityIndex].Value : entities[entityIndex];
            }

            #endregion Private Methods
        }

        #endregion Public Structs
    }
}