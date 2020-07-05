using UnityEditor;

namespace WaynGroup.Mgm.Ability
{
    internal class ScriptTemplates
    {

        public const string TemplatesRoot = "Packages/wayn-group.mgm.ability/Editor/ScriptTemplates";

        [MenuItem("Assets/Create/MGM/Effect Type")]
        public static void CreateAbilityEffectType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/EffectType.txt",
                "NewEffect.cs");
        }
        [MenuItem("Assets/Create/MGM/Cost Type")]
        public static void CreateAbilityCostType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/CostType.txt",
                "NewCost.cs");
        }
    }
}
