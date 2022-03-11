using System;
using System.Text;

using Unity.Entities;

using WaynGroup.Mgm.Ability;

public static class AbilityHelper
{
    #region Public Fields

    public const string ADDRESSABLE_ABILITY_LABEL = "Ability";
    public const string ADDRESSABLE_UiLink_LABEL = "UiLink";

    #endregion Public Fields

    #region Public Methods

    public static uint ComputeAbilityIdFromGuid(string Guid)
    {
        return BitConverter.ToUInt32(Encoding.ASCII.GetBytes(Guid), 0);
    }

    #endregion Public Methods
}