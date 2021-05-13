using System;

using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "AbilityUiLink", menuName = "MGM/AbilityUiLink", order = 1)]
[Serializable]
public class AbilityUiLink : ScriptableObject
{
    public uint Id;
    public GameObject UiPrefab;
    public PanelSettings PanelSettings;
    public VisualTreeAsset UxmlUi;
    public float SortingOrder;


}
