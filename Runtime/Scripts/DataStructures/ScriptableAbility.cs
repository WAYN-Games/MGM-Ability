using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

using WaynGroup.Mgm.Ability;

[Serializable]
public class PrefabAssetRef : AssetReferenceGameObject
{
    public PrefabAssetRef(string guid) : base(guid)
    {
    }
   
    public GameObject GetAssetInEditor()
    {
        if(this.editorAsset != null)
        {
            Debug.Log("Returned from editorAsset");
            return this.editorAsset;
        }
        Debug.Log("Returned from cached asset");
        return (GameObject)this.CachedAsset;
    }
}

[Serializable]
public class SpawnableData : IEffect
{
    #region Public Fields

    public AssetReferenceGameObject PrefabRef;

    public int count;

    [field: SerializeField]
    public TargetingMode Affects { get; set; }
    [field: SerializeField]
    public ActivationPhase Phase { get; set; }

    #endregion Public Fields
}

[Serializable]
public struct AbilityTimings
{
    #region Public Fields

    public float Cast;
    public float CoolDown;

    #endregion Public Fields
}

[Serializable]
public struct Range
{
    #region Public Fields

    public float Min;
    public float Max;

    #endregion Public Fields
}

[CreateAssetMenu(fileName = "NewAbility", menuName = "MGM/Ability", order = 1)]
[Serializable]
public class ScriptableAbility : ScriptableObject
{
    #region Public Fields

    /// <summary>
    /// A unique Id generated when creating the ability in the editor.
    /// </summary>
    public uint Id;

    /// <summary>
    /// The name of the ability.
    /// </summary>
    public LocalizedString Name;

    /// <summary>
    /// The skill Icon.
    /// </summary>
    public Texture2D Icon;

    /// <summary>
    /// The distance constraints that need to be met in order to cast the ability.
    /// </summary>
    public Range Range;

    /// <summary>
    /// The time necessary for the ability to recharge before it can be reused and time needed before it's activation.
    /// </summary>
    public AbilityTimings Timings;

    /// <summary>
    /// The list of costs that are check to cast the ability.
    /// A corresponding effect is automatically added to the list of effect at runtime to consume the associated ressource.
    /// </summary>
    [SerializeReference]
    public List<IAbilityCost> Costs;

    /// <summary>
    /// The list of effect that are applied after the CastTime has elapsed.
    /// </summary>
    [SerializeReference]
    public List<IEffect> Effects;

    #endregion Public Fields

}