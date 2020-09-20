using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;


namespace WaynGroup.Mgm.Ability.UI
{
    public interface ICommand
    {
        void Execute();
    }

    public struct AbilityUICommand : ICommand
    {
        public Entity _owner;
        public int _index;
        public EntityManager _entityManager;

        public void Execute()
        {
            if (Entity.Null.Equals(_owner)) return;
            DynamicBuffer<AbilityBuffer> abbilities = _entityManager.GetBuffer<AbilityBuffer>(_owner);
            if (_index < 0 || _index > abbilities.Length) return;
            Ability ability = abbilities[_index];
            ability.TryCast();
            abbilities[_index] = ability;
        }
    }

    class AbilityUIElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AbilityUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private ScriptableAbility _ability;
        private AbilityUICommand _command;

        public Entity _owner;
        public int _index;
        public EntityManager _entityManager;

        public AbilityUIElement()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUIElement");
            visualTree.CloneTree(this);
            UnassignAbility();
            SetTime(0);
            SetBackGroundFill(0);
            _command = default;
        }


        public void AssignAbility(Entity owner, int index, ScriptableAbility ability, EntityManager entityManager)
        {
            _command._owner = owner;
            _command._index = index;
            _command._entityManager = entityManager;
            _ability = ability;
            this.Q(name: "Icon").style.backgroundImage = new StyleBackground(_ability.Icon);
            this.Q<Button>(name: "abilityButton").clicked += _command.Execute;
        }

        public void UnassignAbility()
        {
            _command = new AbilityUICommand();
            _ability = null;
            this.Q(name: "Icon").style.backgroundImage = null;
            this.Q<Button>(name: "abilityButton").clicked -= _command.Execute;
        }

        public bool IsAssigned => !_command.Equals(default) && _ability != null;

        public void UpdateCoolDown()
        {
            if (!IsAssigned) return;
            (float, float) remainingTime = _command._entityManager.GetBuffer<AbilityBuffer>(_command._owner)[_command._index].Ability.CoolDown.ComputeRemainingTime();
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
