using System;

using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "AbilityUiLink", menuName = "MGM/AbilityUiLink", order = 1)]
[Serializable]
public class AbilityUiLink : ScriptableObject
{
    #region Public Fields

    [HideInInspector]
    public uint Id;

    public GameObject UiPrefab;
    public PanelSettings PanelSettings;
    public VisualTreeAsset UxmlUi;
    public float SortingOrder;

    #endregion Public Fields
}