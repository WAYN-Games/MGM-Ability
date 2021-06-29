
using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{


    class AbilityUITooltip : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AbilityUITooltip, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }


        public AbilityUITooltip()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUITooltip");
            visualTree.CloneTree(this);
            Hide();
            AbilityUIData.Instance.AbilityTooltip = this;
        }

        public void Show(ScriptableAbility ability)
        {
            if (ability == null) return;
            visible = true;
            if (ability.Name.IsEmpty) return;
            this.Q<Label>(name: "Title").text = ability.Name.GetLocalizedString();
        }

        public void Hide()
        {
            visible = false;
            this.Q<Label>(name: "Title").text = "";
        }

    }
}
