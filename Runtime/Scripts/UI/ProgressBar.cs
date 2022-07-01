using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{
    internal class ProgressBar : VisualElement
    {
        #region Private Fields

        private VisualElement backround;
        private VisualElement root;
        private VisualElement fill;
        private Label title;
        private float _value;
        private string _title;

        #endregion Private Fields

        #region Public Constructors

        public ProgressBar()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("ProgressBar");
            visualTree.CloneTree(this);
            root = this.Q<VisualElement>(name: "progressbar-root");
            backround = root.Q<VisualElement>(name: "progressbar-background");
            fill = root.Q<VisualElement>(name: "progressbar-fill");
            title = root.Q<Label>(name: "progressbar-title");
        }

        #endregion Public Constructors

        #region Public Properties

        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                if (IsHorizontal)
                {
                    fill.style.width = new StyleLength(new Length(_value, LengthUnit.Percent));
                }
                else
                {
                    fill.style.height = new StyleLength(new Length(_value, LengthUnit.Percent));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                title.text = _title;
            }
        }

        #endregion Public Properties

        #region Private Properties

        private bool IsHorizontal => root.style.flexDirection.value.Equals(FlexDirection.Row) || root.style.flexDirection.value.Equals(FlexDirection.RowReverse);

        #endregion Private Properties

        #region Public Methods

        public void SetOrientation(FlexDirection orientation)
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
        public void Update()
        {
        }

        #endregion Public Methods

        #region Public Classes

        public new class UxmlFactory : UxmlFactory<ProgressBar, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        { }

        #endregion Public Classes
    }
}