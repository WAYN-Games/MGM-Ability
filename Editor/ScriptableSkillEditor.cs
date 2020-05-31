using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

using WaynGroup.Mgm.Skill;

[CustomEditor(typeof(ScriptableSkill))]
public class ScriptableSkillEditor : Editor
{
    VisualElement root;

    List<Type> EffectTypes;

    PopupField<Type> effectDropDown;

    Type SelectedType;

    SerializedProperty EffectsProperty;

    public void OnEnable()
    {
        CreateInspectorGUI();
    }

    public override VisualElement CreateInspectorGUI()
    {
        Cache();


        LoadBaseLayout();

        MakeEffectTypePicker();

        DisplayEffects();

        return root;
    }

    private void Cache()
    {
        EffectsProperty = serializedObject.FindProperty("Effects");

        EffectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IEffect).IsAssignableFrom(p) && p.IsValueType).ToList();
    }

    private void LoadBaseLayout()
    {
        root = new VisualElement();
        // Import UXML
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/wayn-group.mgm.skill/Editor/ScriptableSkillEditor.uxml");
        TemplateContainer uxmlVe = visualTree.CloneTree();
        root.Add(uxmlVe.contentContainer);
        root.Bind(serializedObject);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/wayn-group.mgm.skill/Editor/ScriptableSkillEditor.uss");
        root.styleSheets.Add(styleSheet);


    }

    private void MakeEffectTypePicker()
    {
        VisualElement effectPickerContainer = root.Query<VisualElement>("effect-picker-container").First();
        effectDropDown = new PopupField<Type>("Effect Type", EffectTypes, 0, (Type t) => t.Name, (Type t) => t.Name);
        SelectedType = effectDropDown.value;
        effectDropDown.RegisterValueChangedCallback(ChangeSelectedType);
        Button addbuton = new Button(AddEffect)
        {
            text = "Add"
        };
        effectDropDown.Add(addbuton);
        effectPickerContainer.Add(effectDropDown);
    }

    private void ChangeSelectedType(ChangeEvent<Type> evt)
    {
        SelectedType = evt.newValue;
    }

    private void AddEffect()
    {
        serializedObject.Update();
        EffectsProperty.arraySize++;
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        EffectsProperty.GetArrayElementAtIndex(EffectsProperty.arraySize - 1).managedReferenceValue = Activator.CreateInstance(SelectedType);
        serializedObject.ApplyModifiedProperties();
        SaveData();
        VisualElement effectsContainer = root.Query<VisualElement>("effects-container").First();
        effectsContainer.Add(BuildGenericElement(SelectedType, EffectsProperty.GetArrayElementAtIndex(EffectsProperty.arraySize - 1), EffectsProperty.arraySize - 1));

    }

    private void DisplayEffects()
    {
        VisualElement effectsContainer = root.Query<VisualElement>("effects-container").First();
        effectsContainer.Clear();

        for (int i = 0; i < EffectsProperty.arraySize; i++)
        {
            SerializedProperty sp = EffectsProperty.GetArrayElementAtIndex(i);
            Type type = EffectTypes.Where(t => $"{t.Assembly.GetName().Name} {t.FullName}".Equals(sp.managedReferenceFullTypename)).FirstOrDefault();
            if (type == null)
            {
                effectsContainer.Add(new Label($"Missing type : {sp.managedReferenceFullTypename}"));
            }
            else
            {
                effectsContainer.Add(BuildGenericElement(type, sp, i));
            }
        }

    }

    private void AddRemoveButton(int index, VisualElement ve)
    {
        ve.Add(new Button(() => { RemoveEffect(index); })
        {
            text = "Remove"
        });
    }

    private VisualElement BuildGenericElement(Type type, SerializedProperty sp, int index)
    {
        Foldout foldout = new Foldout()
        {
            text = type.Name
        };
        foreach (FieldInfo property in type.GetFields().Where(p => p.DeclaringType.Equals(type)))
        {
            SerializedProperty local_sp = sp.FindPropertyRelative(property.Name);
            PropertyField propField = new PropertyField(local_sp)
            {
                bindingPath = local_sp.propertyPath
            };
            propField.Bind(serializedObject);
            foldout.Add(propField);
            AddRemoveButton(index, foldout);
        }
        return foldout;
    }

    private void RemoveEffect(int index)
    {
        serializedObject.Update();
        EffectsProperty.DeleteArrayElementAtIndex(index);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        SaveData();
        DisplayEffects();
    }
    void SaveData()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}
