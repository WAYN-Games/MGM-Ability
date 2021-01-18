using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system update each ability's timming (cast and/or cooldown).
    /// Once a timing is elapsed, the ability state is updated to the next value in the life cycle.
    /// (Casting -> Active -> CoolingDown -> CooledDown -> Casting)
    /// </summary>
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public class AbilityUpdateStateAndTimingsSystem : SystemBase
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

            Entities.ForEach((ref DynamicBuffer<AbilityBufferElement> abilityBuffer, ref AbilityInput abilityInput) =>
            {
                NativeArray<AbilityBufferElement> sbArray = abilityBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    AbilityBufferElement Ability = sbArray[i];
                    Ability = jm.UpdateTiming(DeltaTime, Ability);
                    Ability = jm.UpdateState(Ability, abilityInput);
                    sbArray[i] = Ability;
                }
                abilityInput.Enabled = false;
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
            [ReadOnly] public NativeHashMap<uint, AbilityTimings> TimmingMap;

            public AbilityBufferElement UpdateTiming(float DeltaTime, AbilityBufferElement Ability)
            {
                if (Ability.AbilityState == AbilityState.Casting || Ability.AbilityState == AbilityState.CoolingDown)
                    Ability.CurrentTimming -= DeltaTime;
                return Ability;
            }

            public AbilityBufferElement UpdateState(AbilityBufferElement ability, AbilityInput abilityInput)
            {
                if (abilityInput.AbilityId == ability.Guid && abilityInput.Enabled)
                {
                    bool canCast = true;
                    if (ability.AbilityState != AbilityState.CooledDown)
                    {
                        canCast = false;
                        Debug.Log($"Ability not ready yet.");
                    }
                    /*if (!ability.IsInRange)
                    {
                        canCast = false;
                        Debug.Log($"Target out of range.");
                    }*/
                    if (!ability.HasEnougthRessource)
                    {
                        canCast = false;
                        Debug.Log($"Not enougth ressources.");
                    }
                    if (canCast)
                    {
                        ability = StartCasting(ability);
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
                    ability = StartCoolDown(ability);
                    return ability;
                }

                bool IsCastComplete = ability.AbilityState == AbilityState.Casting && ability.CurrentTimming < 0;

                if (IsCastComplete)
                {
                    ability = Activate(ability);
                    return ability;
                }

                return ability;
            }

            public AbilityBufferElement WaitForActivation(AbilityBufferElement Ability)
            {
                Ability.AbilityState = AbilityState.CooledDown;
                return Ability;
            }

            public AbilityBufferElement StartCasting(AbilityBufferElement Ability)
            {
                if (TimmingMap.TryGetValue(Ability.Guid, out AbilityTimings timming))
                {
                    Ability.AbilityState = AbilityState.Casting;
                    Ability.CurrentTimming = timming.Cast;
                }
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

            public void UpdateCachedMap(NativeHashMap<uint, AbilityTimings> tmpMap)
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


        private NativeHashMap<uint, AbilityTimings> BuildTimingMap(Dictionary<uint, ScriptableAbility> abilityCatalog)
        {
            NativeHashMap<uint, AbilityTimings> tmpMap = new NativeHashMap<uint, AbilityTimings>(abilityCatalog.Count, Allocator.Persistent);
            foreach (KeyValuePair<uint, ScriptableAbility> keyValuePair in abilityCatalog)
            {
                ScriptableAbility scriptableAbility = keyValuePair.Value;
                tmpMap.Add(scriptableAbility.Id, scriptableAbility.Timings);
            }

            return tmpMap;
        }

        private void ListenForAbilityCatalogUpdate()
        {
            Enabled = false; // Avoid system update with not ready catalog.
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate += UpdpateCatalog;
        }

        private void UpdpateCatalog(Dictionary<uint, ScriptableAbility> abilityCatalog)
        {

            NativeHashMap<uint, AbilityTimings> tmpMap = BuildTimingMap(abilityCatalog);
            _jm.UpdateCachedMap(tmpMap);

            Enabled = true; // Catalog is ready, so sytem can update.
        }
        #endregion
    }
}
