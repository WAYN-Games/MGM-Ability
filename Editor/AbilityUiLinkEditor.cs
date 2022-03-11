using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

[CustomEditor(typeof(AbilityUiLink))]
public class AbilityUiLinkEditor : Editor
{
    #region Public Methods

    public void OnEnable()
    {
        RegisterAsAddressable((AbilityUiLink)target);
    }

    public void RegisterAsAddressable(AbilityUiLink uiLink)
    {
        string Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target));

        if (string.IsNullOrEmpty(Guid)) return;

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (!settings.GetLabels().Contains(AbilityHelper.ADDRESSABLE_UiLink_LABEL))
        {
            settings.AddLabel(AbilityHelper.ADDRESSABLE_UiLink_LABEL);
        }

        uint GuidHash = AbilityHelper.ComputeAbilityIdFromGuid(Guid);
        AddressableAssetEntry entry = settings.FindAssetEntry(Guid);

        if (entry != null)
        {
            if (GuidHash != uiLink.Id)
            {
                uiLink.Id = GuidHash;
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
            entry.labels.Add(AbilityHelper.ADDRESSABLE_UiLink_LABEL);
            entry.address = target.name;
            uiLink.Id = GuidHash;
            //You'll need these to run to save the changes!
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }
        AssetDatabase.SaveAssets();
    }

    #endregion Public Methods
}