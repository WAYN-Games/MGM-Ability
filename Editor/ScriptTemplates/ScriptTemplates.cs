using UnityEditor;

namespace WaynGroup.Mgm.Ability
{
    internal class ScriptTemplates
    {

        public const string TemplatesRoot = "Packages/wayn-group.mgm.ability/Editor/ScriptTemplates";

        [MenuItem("Assets/Create/MGM/Effect Type")]
        public static void CreateRuntimeComponentType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/EffectType.txt",
                "NewEffect.cs");
        }
    }
}
