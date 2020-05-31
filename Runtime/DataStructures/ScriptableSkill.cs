using System;
using System.Collections.Generic;

using UnityEngine;

using WaynGroup.Mgm.Skill;

[CreateAssetMenu(fileName = "NewSkill", menuName = "MGM/Skill", order = 1)]
public class ScriptableSkill : ScriptableObject
{
    public Guid Id = new Guid();
    public string Name;
    public float CoolDown;
    public float CastTime;
    public int Test;
    [SerializeReference]
    public List<IEffect> Effects;

}
