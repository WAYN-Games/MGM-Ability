using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability;


[CustomEditor(typeof(ScriptableAbility))]
public class ScriptableAbilityEditor : Editor
{
    VisualElement root;

    List<Type> EffectTypes;
    List<Type> CostTypes;

    PopupField<Type> effectDropDown;

    PopupField<Type> costDropDown;

    Type SelectedEffectType;

    Type SelectedCostType;

    SerializedProperty EffectsProperty;

    SerializedProperty CostsProperty;
    SerializedProperty SpawnablesProperty;


    private readonly string[] _costStirngParams = new string[] { "costs-container", "Undefined ability cost type on {0} cost.", "Undefined ability cost type to remove" };
    private readonly string[] _effectStirngParams = new string[] { "effects-container", "Undefined ability effect type on {0} effect.", "Undefined effect type to remove" };




    public void OnEnable()
    {
        Undo.RecordObject(target, "Ability Change");
        Undo.undoRedoPerformed += RefreshInspector;
    }

    private void RefreshInspector()
    {
        CreateInspectorGUI();
    }
    public void OnDisable()
    {
        Undo.undoRedoPerformed -= RefreshInspector;
    }
    public override VisualElement CreateInspectorGUI()
    {
        Initialization();

        Cache();

        LoadBaseLayout();
        MakeCostTypePicker();

        MakeEffectTypePicker();

        Display(EffectsProperty, EffectTypes, _effectStirngParams);
        Display(CostsProperty, CostTypes, _costStirngParams);

        MakeSpawnablesList();
        SaveData();
        return root;
    }

    private void MakeSpawnablesList()
    {
        VisualElement spawnablesContainer = root.Query<VisualElement>("spawnables-container").First();
        PropertyField propertyField = new PropertyField(SpawnablesProperty);
        propertyField.Bind(serializedObject);
        spawnablesContainer.Add(propertyField);
    }

    private void Initialization()
    {
        ScriptableAbility ability = (ScriptableAbility)target;
        Undo.RecordObject(ability, "Ability Change");
        RegisterAsAddressable(ability);
    }
    public void RegisterAsAddressable(ScriptableAbility ability)
    {
        string Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target));

        if (string.IsNullOrEmpty(Guid)) return;

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (!settings.GetLabels().Contains(AbilityHelper.ADDRESSABLE_ABILITY_LABEL))
        {
            settings.AddLabel(AbilityHelper.ADDRESSABLE_ABILITY_LABEL);
        }

        uint GuidHash = AbilityHelper.ComputeAbilityIdFromGuid(Guid);
        AddressableAssetEntry entry = settings.FindAssetEntry(Guid);

        if (entry != null)
        {
            if (GuidHash != ability.Id)
            {
                ability.Id = GuidHash;
                AssetDatabase.SaveAssets();
            }
            return;

        }


        AddressableAssetGroup grp = settings.FindGroup("MGM-Abilities");
        if (grp == null)
        {
            grp = settings.CreateGroup("MGM-Abilities", false, false, false, settings.DefaultGroup.Schemas);
        }

        entry = settings.CreateOrMoveEntry(Guid, grp);
        if (entry != null)
        {
            entry.labels.Add(AbilityHelper.ADDRESSABLE_ABILITY_LABEL);
            entry.address = ability.name;
            ability.Id = GuidHash;
            //You'll need these to run to save the changes!
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }
        AssetDatabase.SaveAssets();

    }



    private void Cache()
    {

        EffectsProperty = serializedObject.FindProperty("Effects");

        EffectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IEffect).IsAssignableFrom(p) && p.IsValueType).ToList();

        CostsProperty = serializedObject.FindProperty("Costs");

        SpawnablesProperty = serializedObject.FindProperty("Spawnables");

        CostTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IAbilityCost).IsAssignableFrom(p) && p.IsValueType).ToList();
    }

    private void LoadBaseLayout()
    {
        root = new VisualElement();
        // Import UXML
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/wayn-group.mgm.ability/Editor/ScriptableAbilityEditor.uxml");
        TemplateContainer uxmlVe = visualTree.CloneTree();
        root.Add(uxmlVe.contentContainer);
        root.Bind(serializedObject);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/wayn-group.mgm.ability/Editor/ScriptableAbilityEditor.uss");
        root.styleSheets.Add(styleSheet);


    }

    private void MakeEffectTypePicker()
    {
        VisualElement effectPickerContainer = root.Query<VisualElement>("effect-picker-container").First();
        if (EffectTypes.Count == 0)
        {
            effectPickerContainer.Add(new HelpBox("Your project does not define any effect types.", HelpBoxMessageType.Warning));
            return;
        }
        effectDropDown = new PopupField<Type>("Effect Type", EffectTypes, 0, (Type t) => t.Name, (Type t) => t.Name);
        SelectedEffectType = effectDropDown.value;
        effectDropDown.RegisterValueChangedCallback(ChangeSelectedEffectType);
        Button addbuton = new Button(() => Add(EffectsProperty, EffectTypes, _effectStirngParams, SelectedEffectType, false))
        {
            text = "Add"
        };
        effectDropDown.Add(addbuton);
        effectPickerContainer.Add(effectDropDown);
    }

    private void MakeCostTypePicker()
    {
        VisualElement costPickerContainer = root.Query<VisualElement>("cost-picker-container").First();
        if (CostTypes.Count == 0)
        {
            costPickerContainer.Add(new HelpBox("Your project does not define any cost types.", HelpBoxMessageType.Warning));
            return;
        }

        costDropDown = new PopupField<Type>("Cost Type", CostTypes, 0, (Type t) => t.Name, (Type t) => t.Name);
        SelectedCostType = costDropDown.value;
        costDropDown.RegisterValueChangedCallback(ChangeSelectedCostType);
        Button addbuton = new Button(() => Add(CostsProperty, CostTypes, _costStirngParams, SelectedCostType, true))
        {
            text = "Add"
        };
        costDropDown.Add(addbuton);
        costPickerContainer.Add(costDropDown);
    }

    private void ChangeSelectedCostType(ChangeEvent<Type> evt)
    {
        SelectedCostType = evt.newValue;
    }

    private void ChangeSelectedEffectType(ChangeEvent<Type> evt)
    {
        SelectedEffectType = evt.newValue;
    }

    private void Add(SerializedProperty listProperty, List<Type> types, string[] stringParams, Type selectedType, bool IsCost)
    {
        serializedObject.Update();
        listProperty.arraySize++;
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).managedReferenceValue = Activator.CreateInstance(selectedType);
        serializedObject.ApplyModifiedProperties();
        SaveData();
        Display(listProperty, types, stringParams);
    }

    private void MakeSpawnableList()
    {

    }

    private void Display(SerializedProperty listPorperty, List<Type> types, string[] paramStrings)
    {
        VisualElement container = root.Query<VisualElement>(paramStrings[0]).First();
        container.Clear();

        for (int i = 0; i < listPorperty.arraySize; ++i)
        {
            int index = i;
            SerializedProperty sp = listPorperty.GetArrayElementAtIndex(index);
            Type type = types.Where(t => $"{t.Assembly.GetName().Name} {t.FullName}".Equals(sp.managedReferenceFullTypename)).FirstOrDefault();
            if (type == null)
            {
                Debug.LogWarning(string.Format(paramStrings[1], serializedObject.targetObject.name));
                container.Add(new Button(() => { Remove(index, listPorperty, types, paramStrings); })
                {
                    text = paramStrings[2]
                });
            }
            else
            {
                VisualElement ve = new VisualElement();
                VisualElement foldout = BuildGenericElement(type, sp);
                ve.style.flexDirection = FlexDirection.Row;
                foldout.style.flexGrow = 1;
                ve.Add(foldout);
                Button b = new Button(() => { Remove(index, listPorperty, types, paramStrings); })
                {
                    text = "-"
                };
                b.style.height = 12;
                ve.Add(b);
                container.Add(ve);
            }
        }
    }




    private VisualElement BuildGenericElement(Type type, SerializedProperty sp)
    {
        Foldout foldout = new Foldout()
        {
            text = type.Name
        };
        foreach (PropertyInfo property in type.GetProperties().Where(p => p.DeclaringType.Equals(type)))
        {
            SerializedProperty local_sp = sp.FindPropertyRelative(string.Format("<{0}>k__BackingField", property.Name));
            if (local_sp == null) continue;
            PropertyField propField = new PropertyField(local_sp, property.Name)
            {
                bindingPath = local_sp.propertyPath
            };
            propField.Bind(serializedObject);
            foldout.Add(propField);
        }
        foreach (FieldInfo property in type.GetFields().Where(p => p.DeclaringType.Equals(type)))
        {
            SerializedProperty local_sp = sp.FindPropertyRelative(property.Name);
            if (local_sp == null) continue;
            PropertyField propField = new PropertyField(local_sp)
            {
                bindingPath = local_sp.propertyPath
            };
            propField.Bind(serializedObject);
            foldout.Add(propField);
        }
        return foldout;
    }

    private void Remove(int index, SerializedProperty listPorperty, List<Type> types, string[] paramStrings)
    {
        serializedObject.Update();
        listPorperty.DeleteArrayElementAtIndex(index);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        SaveData();
        Display(listPorperty, types, paramStrings);
    }

    void SaveData()
    {
        Undo.RecordObject(target, "Ability Change");
        AssetDatabase.SaveAssets();
    }

    // This method allow to dynamicaly override the preview icon bath in hte inspector window and the project view to the ability ingame icon.
    // In the porject window, if the view is to small it will default to the unity scriptable object icon.
    // If the ability in game icon is not set, it will default to the unity scriptable object icon. 
    public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
    {
        ScriptableAbility ability = (ScriptableAbility)target;

        if (ability == null || ability.Icon == null)
            return null;

        Texture2D tex = new Texture2D(width, height);
        EditorUtility.CopySerialized(ability.Icon, tex);

        return tex;
    }
}
