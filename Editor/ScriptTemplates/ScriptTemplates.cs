using UnityEditor;

namespace WaynGroup.Mgm.Ability
{
    internal class ScriptTemplates
    {
        #region Public Fields

        public const string TemplatesRoot = "Packages/wayn-group.mgm.ability/Editor/ScriptTemplates";

        #endregion Public Fields

        #region Public Methods

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

        #endregion Public Methods
    }
}