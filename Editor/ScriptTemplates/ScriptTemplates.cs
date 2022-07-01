using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    internal class ScriptTemplates
    {
        #region Public Fields

        public static string TemplatesRoot = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof(ScriptTemplates))).resolvedPath;

        #endregion Public Fields

        #region Public Methods

        [MenuItem("Assets/Create/MGM/Effect Type")]
        public static void CreateAbilityEffectType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/Editor/ScriptTemplates/EffectType.txt",
                "NewEffect.cs");
        }

        [MenuItem("Assets/Create/MGM/Cost Type")]
        public static void CreateAbilityCostType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/Editor/ScriptTemplates/CostType.txt",
                "NewCost.cs");
        }

        #endregion Public Methods
    }
}