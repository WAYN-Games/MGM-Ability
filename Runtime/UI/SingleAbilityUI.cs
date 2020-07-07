
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;

class SingleAbilityUI : VisualElement
{

    private Entity _abilityOwner = Entity.Null;
    private int _abilityIndex = -1;
    private EntityManager _entityManager;
    private bool _isSlotAssigned;

    private VisualElement _abilitySlot;
    private VisualElement _cooldownBackgroundUI;
    private Label _cooldownTextUI;
    private VisualElement _iconUI;

    public void AssignAbility(Entity abilityOwner, int abilityIndex)
    {
        _abilityOwner = abilityOwner;
        _abilityIndex = abilityIndex;
        _isSlotAssigned = true;
        visible = true;
    }

    public void UnassignAbility()
    {
        _abilityOwner = Entity.Null;
        _abilityIndex = -1;
        _isSlotAssigned = false;
        visible = false;
    }

    public void UpdateCooldowns(float timeRemaing, float percentageremaining)
    {
        _cooldownBackgroundUI.style.height = _abilitySlot.worldBound.height * percentageremaining;
        _cooldownTextUI.text = $"{math.round(timeRemaing * 10) / 10}";
        if (timeRemaing <= 0)
        {
            _cooldownTextUI.visible = false;
        }
        else
        {
            _cooldownTextUI.visible = true;
        }
    }



    public new class UxmlFactory : UxmlFactory<SingleAbilityUI, UxmlTraits> { }


    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            SingleAbilityUI root = ve as SingleAbilityUI;

            root.Clear();

            root._abilitySlot = new VisualElement();
            root._abilitySlot.AddToClassList("abilitySlotShape");
            root.Add(root._abilitySlot);

            root._iconUI = new VisualElement();
            root.Add(root._iconUI);

            VisualElement coolDown = new VisualElement();
            VisualElement filler = new VisualElement();
            filler.style.flexGrow = 1;
            filler.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
            coolDown.Add(filler);

            VisualElement coolDownBar = new VisualElement();
            coolDownBar.AddToClassList("coolDownBar");
            coolDown.Add(coolDownBar);
            root._cooldownBackgroundUI = coolDownBar;

            root.Add(coolDown);

            root._cooldownTextUI = new Label();
            root.Add(root._cooldownTextUI);
        }
    }

}
