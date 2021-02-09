﻿
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
        }

        public void Show(ScriptableAbility ability)
        {
            visible = true;
            this.Q<Label>(name: "Title").text = ability.name;
        }

        public void Hide()
        {
            visible = false;
            this.Q<Label>(name: "Title").text = "";
        }

    }
}
