namespace WaynGroup.Mgm.Skill
{
    internal class ScriptTemplates
    {
        public const string TemplatesRoot = "Packages/com.unity.entities/Unity.Entities.Editor/ScriptTemplates";

        [MenuItem("Assets/Create/ECS/Runtime Component Type")]
        public static void CreateRuntimeComponentType()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"{TemplatesRoot}/RunTimeComponent.txt",
                "NewComponent.cs");
        }
    }
}
