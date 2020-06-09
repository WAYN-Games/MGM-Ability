using System;
using System.Collections.Generic;

using UnityEngine;

using WaynGroup.Mgm.Skill;

[CreateAssetMenu(fileName = "NewSkill", menuName = "MGM/Skill", order = 1)]
public class ScriptableSkill : ScriptableObject
{
    /// <summary>
    /// A unique Id generated when creating the skill in the editor.
    /// </summary>
    public Guid Id = new Guid();
    /// <summary>
    /// The name of the skill.
    /// </summary>
    public string Name;
    /// <summary>
    /// The distance constraints that need to be met in order to cast the skill.
    /// </summary>
    public Range Range;
    /// <summary>
    /// The time necessary for the skill to recahrge before it can be reused.
    /// </summary>
    public float CoolDown;
    /// <summary>
    /// The time needed by the skill between the user input and the trigerring of the skill effect.
    /// </summary>
    public float CastTime;

    /// <summary>
    /// The list of effect that are applied after the CastTime has elapsed.
    /// </summary>
    [SerializeReference]
    public List<IEffect> Effects;

}

[Serializable]
public struct Range
{
    public float Min;
    public float Max;
}
