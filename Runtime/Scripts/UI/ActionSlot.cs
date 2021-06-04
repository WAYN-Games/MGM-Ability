using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{


    class ActionSlot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ActionSlot, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public ActionSlot()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("ActionSlot");
            visualTree.CloneTree(this);
            AbilityUIData.Instance.AddDropArea(this);
        }


    }


    public sealed class AbilityUIData
    {
        private List<VisualElement> DropAreas = new List<VisualElement>();
        internal AbilityUITooltip AbilityTooltip;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AbilityUIData()
        {
        }

        private AbilityUIData()
        {
        }

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

        public static AbilityUIData Instance { get; } = new AbilityUIData();
    }
}
