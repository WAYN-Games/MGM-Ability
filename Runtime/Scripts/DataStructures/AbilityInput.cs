using System;

using Unity.Entities;
using UnityEngine;

[Serializable]
public struct AbilityInput : IComponentData
{
    #region Public Fields

    public uint AbilityId;

    /// <summary>
    /// Store the enabeling state in the for inputed ability.
    /// If the ability can't be cast due to several restriction each is represented as a bitmask value.
    /// 0 and 1 represent respectively enable and disabled states.
    /// </summary>
    public uint _enableMask;

    #endregion Public Fields

    #region Public Constructors

    public AbilityInput(uint abilityId) : this()
    {
        AbilityId = abilityId;
        _enableMask = 1;
    }

    #endregion Public Constructors

    #region Public Methods

    public void AddRestriction(uint restrictionId) => _enableMask = +restrictionId;

    /// <summary>
    /// Indicate if the ability should be casted.
    /// If the mask value is not equal to 0, either there were not cast request
    /// or the requested ability can't be cast due to restrictions.
    /// </summary>
    /// <returns></returns>
    public bool IsApplicable() => _enableMask == 0;

    public bool IsEnabled() => (_enableMask & 1) == 0;

    public void Enable() => _enableMask = 0;

    public void Disable() => _enableMask = 1;

    #endregion Public Methods
}