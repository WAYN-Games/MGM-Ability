using Unity.Entities;
using Unity.Mathematics;
using Unity.UIElements.Runtime;

using UnityEngine;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability;

public class AbilitySlot : MonoBehaviour
{
    private VisualElement _abilitySlot;
    private VisualElement _cooldownBackgroundUI;
    private Label _cooldownTextUI;

    private Entity _abilityOwner = Entity.Null;
    private int _abilityIndex = -1;
    private EntityManager _entityManager;
    private bool _isSlotAssigned;

    public void AssignAbility(Entity abilityOwner, int abilityIndex)
    {
        _abilityOwner = abilityOwner;
        _abilityIndex = abilityIndex;
        _isSlotAssigned = true;
    }

    public void UnassignAbility()
    {
        _abilityOwner = Entity.Null;
        _abilityIndex = -1;
        _isSlotAssigned = false;
    }

    private void Start()
    {
        VisualElement root = GetComponent<PanelRenderer>().visualTree;
        _abilitySlot = root.Q(name: AbilitiesUiConstants.ABILITY_SLOT_ROOT_UI_NAME);
        _cooldownBackgroundUI = root.Q(name: AbilitiesUiConstants.ABILITY_SLOT_COOLDOWN_BACKGROUND_UI_NAME);
        _cooldownTextUI = root.Q<Label>(name: AbilitiesUiConstants.ABILITY_SLOT_COOLDOWN_TEXT_UI_NAME);
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {

        if (!_isSlotAssigned) return;

        if (!_entityManager.Exists(_abilityOwner))
        {
            UnassignAbility();
            return;
        }

        if (_abilityIndex < 0)
        {
            UnassignAbility();
            return;
        }

        DynamicBuffer<AbilityBuffer> abilities = _entityManager.GetBuffer<AbilityBuffer>(_abilityOwner);

        if (abilities.Length < _abilityIndex)
        {
            UnassignAbility();
            return;
        }


        (float, float) remains = _entityManager.GetBuffer<AbilityBuffer>(_abilityOwner)[_abilityIndex].Ability.CoolDown.ComputeRemainingTime();
        UpdateCooldownUi(remains.Item1, remains.Item2);
    }

    public void UpdateCooldownUi(float timeRemaing, float percentageremaining)
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
}
