using UnityEditor;

namespace WaynGroup.Mgm.Skill
{
    internal class ScriptTemplates
    {

        public const string TemplatesRoot = "Packages/wayn-group.mgm.skill/Editor/ScriptTemplates";

        [MenuItem("Assets/Create/MGM/Effect Type")]
        public static void CreateRuntimeComponentType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/EffectType.txt",
                "NewEffect.cs");
        }
    }
}
