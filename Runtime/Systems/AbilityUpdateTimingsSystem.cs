using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;



namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system update each ability's timming (cast and/or cooldown).
    /// Once a timing is elapsed, the ability state is updated to the next value in the life cycle.
    /// (Casting -> Active -> CoolingDown -> CooledDown -> Casting)
    /// </summary>
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public class AbilityUpdateTimingsSystem : SystemBase
    {
        /// <summary>
        /// Struct holding methods encapsulating logic used by the Job in a human readable way.
        /// This struct is used to help 'self document' the code.
        /// </summary>
        private JobMethods _jm;

        protected override void OnCreate()
        {
            base.OnCreate();
            _jm = new JobMethods();
            ListenForAbilityCatalogUpdate();
        }

        protected override void OnUpdate()
        {
            float DeltaTime = World.Time.DeltaTime;
            JobMethods jm = _jm;

            Entities.ForEach((ref DynamicBuffer<AbilityBufferElement> abilityBuffer) =>
            {
                NativeArray<AbilityBufferElement> sbArray = abilityBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    AbilityBufferElement Ability = sbArray[i];
                    Ability = jm.UpdateTiming(DeltaTime, Ability);
                    Ability = jm.UpdateState(Ability);
                    sbArray[i] = Ability;
                }
            }).WithBurst()
            .ScheduleParallel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _jm.TimmingMap.Dispose();
        }

        #region Self Documenting Encapsulations
        /// <summary>
        /// Struct holding methods encapsulating logic used by the Job in a human readable way.
        /// This struct is used to help 'self document' the code.
        /// </summary>
        private struct JobMethods
        {
            /// <summary>
            /// A cached native hashmap of default timings for each ability.
            /// </summary>
            [ReadOnly] public NativeHashMap<Guid, AbilityTimings> TimmingMap;

            public AbilityBufferElement UpdateTiming(float DeltaTime, AbilityBufferElement Ability)
            {
                if (Ability.AbilityState == AbilityState.Casting || Ability.AbilityState == AbilityState.CoolingDown)
                    Ability.CurrentTimming -= DeltaTime;
                return Ability;
            }

            public AbilityBufferElement UpdateState(AbilityBufferElement Ability)
            {
                if (Ability.AbilityState == AbilityState.CooledDown)
                {
                    Ability = StartCasting(Ability);
                    return Ability;
                }

                bool IsCooldownComplete = Ability.AbilityState == AbilityState.CoolingDown && Ability.CurrentTimming < 0;
                if (IsCooldownComplete)
                {
                    Ability = WaitForActivation(Ability);
                    return Ability;
                }

                if (Ability.AbilityState == AbilityState.Active)
                {
                    Ability = StartCoolDown(Ability);
                    return Ability;
                }

                bool IsCastComplete = Ability.AbilityState == AbilityState.Casting && Ability.CurrentTimming < 0;

                if (IsCastComplete)
                {
                    Ability = Activate(Ability);
                    return Ability;
                }

                return Ability;
            }

            public AbilityBufferElement WaitForActivation(AbilityBufferElement Ability)
            {
                Ability.AbilityState = AbilityState.CooledDown;
                return Ability;
            }

            public AbilityBufferElement StartCasting(AbilityBufferElement Ability)
            {
                Ability.AbilityState = AbilityState.Casting;
                Ability.CurrentTimming = TimmingMap[Ability.Guid].Cast;
                return Ability;
            }

            public AbilityBufferElement Activate(AbilityBufferElement Ability)
            {
                Ability.AbilityState = AbilityState.Active;
                return Ability;
            }

            public AbilityBufferElement StartCoolDown(AbilityBufferElement Ability)
            {
                Ability.AbilityState = AbilityState.CoolingDown;
                Ability.CurrentTimming = TimmingMap[Ability.Guid].CoolDown;
                return Ability;
            }

            public void UpdateCachedMap(NativeHashMap<Guid, AbilityTimings> tmpMap)
            {
                DisposeOfPrevioudCacheIfExists();
                TimmingMap = tmpMap;
            }

            private void DisposeOfPrevioudCacheIfExists()
            {
                if (TimmingMap.IsCreated)
                {
                    TimmingMap.Dispose();
                }
            }
        }


        private NativeHashMap<Guid, AbilityTimings> BuildTimingMap(List<ScriptableAbility> abilityCatalog)
        {
            NativeHashMap<Guid, AbilityTimings> tmpMap = new NativeHashMap<Guid, AbilityTimings>(abilityCatalog.Count, Allocator.Persistent);
            foreach (ScriptableAbility scriptableAbility in abilityCatalog)
            {
                tmpMap.Add(new Guid(scriptableAbility.Id), new AbilityTimings() { Cast = scriptableAbility.CastTime, CoolDown = scriptableAbility.CoolDown });
            }

            return tmpMap;
        }

        private struct AbilityTimings
        {
            public float Cast;
            public float CoolDown;
        }
        private void ListenForAbilityCatalogUpdate()
        {
            Enabled = false; // Avoid system update with not ready catalog.
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate += UpdpateCatalog;
        }

        private void UpdpateCatalog(List<ScriptableAbility> abilityCatalog)
        {
            NativeHashMap<Guid, AbilityTimings> tmpMap = BuildTimingMap(abilityCatalog);
            _jm.UpdateCachedMap(tmpMap);

            Enabled = true; // Catalog is ready, so sytem can update.
        }
        #endregion
    }
}
