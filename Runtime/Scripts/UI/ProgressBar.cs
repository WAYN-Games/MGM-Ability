
using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{


    class ProgressBar : VisualElement
    {
        private VisualElement backround;
        private VisualElement root;
        private VisualElement fill;

        public new class UxmlFactory : UxmlFactory<ProgressBar, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private bool IsHorizontal => root.style.flexDirection.value.Equals(FlexDirection.Row) || root.style.flexDirection.value.Equals(FlexDirection.RowReverse);
        public ProgressBar()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("ProgressBar");
            visualTree.CloneTree(this);
            root = this.Q<VisualElement>(name: "progressbar-root");
            backround = root.Q<VisualElement>(name: "progressbar-background");
            fill = root.Q<VisualElement>(name: "progressbar-fill");
            SetProgresss(50f);


            Color c = fill.style.backgroundColor.value;
            c.a = 1;
            fill.style.backgroundColor = new StyleColor(c);
        }

        public void SetOrinetation(FlexDirection orientation)
        {

            if ((orientation.Equals(FlexDirection.Column) || orientation.Equals(FlexDirection.ColumnReverse))
                && IsHorizontal)
            {
                fill.style.height = fill.style.width;
                fill.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            }

            if ((orientation.Equals(FlexDirection.Row) || orientation.Equals(FlexDirection.RowReverse))
                && !IsHorizontal)
            {
                fill.style.width = fill.style.height;
                fill.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            }
            root.style.flexDirection = orientation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent">value between 0 and 100</param>
        public void SetProgresss(float percent)
        {
            fill.style.width = new StyleLength(new Length(percent, LengthUnit.Percent));
        }
    }
}
