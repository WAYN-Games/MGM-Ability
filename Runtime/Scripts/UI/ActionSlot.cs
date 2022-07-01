using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{
    public sealed class AbilityUIData
    {
        #region Internal Fields

        internal AbilityUITooltip AbilityTooltip;

        #endregion Internal Fields

        #region Private Fields

        private List<VisualElement> DropAreas = new List<VisualElement>();

        #endregion Private Fields

        #region Public Constructors

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AbilityUIData()
        {
        }

        #endregion Public Constructors

        #region Private Constructors

        private AbilityUIData()
        {
        }

        #endregion Private Constructors

        #region Public Properties

        public static AbilityUIData Instance { get; } = new AbilityUIData();

        #endregion Public Properties

        #region Public Methods

        public void AddDropArea(VisualElement dropArea)
        {
            DropAreas.Add(dropArea);
        }

        public void RemoveDropArea(VisualElement dropArea)
        {
            DropAreas.Remove(dropArea);
        }

        public VisualElement FindDropArea(Vector2 mousePosition)
        {
            return DropAreas.Where(x => x.worldBound.Contains(mousePosition)).FirstOrDefault();
        }

        #endregion Public Methods
    }

    internal class ActionSlot : VisualElement
    {
        #region Public Constructors

        public ActionSlot()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("ActionSlot");
            visualTree.CloneTree(this);
            AbilityUIData.Instance.AddDropArea(this);
        }

        #endregion Public Constructors

        #region Public Classes

        public new class UxmlFactory : UxmlFactory<ActionSlot, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        { }

        #endregion Public Classes
    }
}