using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;


namespace WaynGroup.Mgm.Ability.UI
{
    class AbilityUIElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AbilityUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private ScriptableAbility _ability;
        private Entity _owner;
        private int _index;
        private EntityManager _entityManager;

        public AbilityUIElement()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUIElement");
            visualTree.CloneTree(this);
            UnassignAbility();
            SetTime(0);
            SetBackGroundFill(0);
        }

        public void Execute()
        {
            Debug.Log($"Executing ability {_index}");
            if (Entity.Null.Equals(_owner) || _ability == null) return;
            DynamicBuffer<AbilityBuffer> abbilities = _entityManager.GetBuffer<AbilityBuffer>(_owner);
            Ability ability = abbilities[_index];
            ability.TryCast();
            abbilities[_index] = ability;
        }

        public void AssignAbility(Entity owner, int index, ScriptableAbility ability, EntityManager entityManager)
        {
            _owner = owner;
            _index = index;
            _ability = ability;
            _entityManager = entityManager;
            this.Q(name: "Icon").style.backgroundImage = new StyleBackground(_ability.Icon);
            this.Q<Button>(name: "abilityButton").clicked += Execute;
        }

        public void UnassignAbility()
        {
            _owner = Entity.Null;
            _ability = null;
            this.Q(name: "Icon").style.backgroundImage = null;
            this.Q<Button>(name: "abilityButton").clicked -= Execute;
        }

        public bool IsAssigned => Entity.Null != _owner && _ability != null;

        public void UpdateCoolDown()
        {
            if (!IsAssigned) return;
            (float, float) remainingTime = _entityManager.GetBuffer<AbilityBuffer>(_owner)[_index].Ability.CoolDown.ComputeRemainingTime();
            SetTime(remainingTime.Item1);
            SetBackGroundFill(remainingTime.Item2);
        }

        private void SetBackGroundFill(float fill)
        {
            VisualElement bg = this.Q(name: "coolDownBackground");

            if (fill <= 0)
            {
                bg.visible = false;
                return;
            }
            if (!bg.visible) bg.visible = true;

            bg.style.height = bg.parent.worldBound.height * math.clamp(fill, 0, 1);
        }

        private void SetTime(float seconds)
        {
            Label label = this.Q<Label>(name: "coolDownTime");

            if (seconds <= 0)
            {
                label.visible = false;
                return;
            }
            if (!label.visible) label.visible = true;
            seconds = math.round(seconds * 10) / 10;
            label.text = $"{seconds}s";
        }

        private void SetIcon(Texture2D icon)
        {
            this.Q(name = "Icon").style.backgroundImage = new StyleBackground(icon);
        }

    }
}
