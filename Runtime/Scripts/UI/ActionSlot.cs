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
            DragAndDropData.Instance.AddDropArea(this);
        }


    }


    public sealed class DragAndDropData
    {
        private List<VisualElement> DropAreas = new List<VisualElement>();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DragAndDropData()
        {
        }

        private DragAndDropData()
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

        public VisualElement FindClosestDropArea(VisualElement elepentToDrop)
        {
            return DropAreas.Where(x => x.worldBound.Overlaps(elepentToDrop.worldBound)).OrderBy(x => Vector2.Distance(x.worldBound.position, elepentToDrop.worldBound.position)).FirstOrDefault();
        }

        public static DragAndDropData Instance { get; } = new DragAndDropData();
    }
}
