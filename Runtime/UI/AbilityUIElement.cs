
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;


namespace WaynGroup.Mgm.Ability.UI
{
    [Preserve]
    class AbilityUIElement : VisualElement
    {
        [Preserve]
        public new class UxmlFactory : UxmlFactory<AbilityUIElement, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        [Preserve]
        public void Init()
        {
            UnassignAbility();
            SetTime(0);
            SetBackGroundFill(0);
        }

        [Preserve]
        private ScriptableAbility _ability;
        [Preserve]
        private Entity _owner;
        [Preserve]
        private int _index;
        [Preserve]
        private EntityManager _entityManager;

        public AbilityUIElement()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUIElement");
            visualTree.CloneTree(this);
            this.Q<AbilityUIElement>().Init();
        }

        [Preserve]
        public void AssignAbility(Entity owner, int index, ScriptableAbility ability, EntityManager entityManager)
        {
            _owner = owner;
            _index = index;
            _ability = ability;
            _entityManager = entityManager;
            this.Q(name: "Icon").style.backgroundImage = new StyleBackground(_ability.Icon);
        }

        [Preserve]
        public void UnassignAbility()
        {
            _owner = Entity.Null;
            _ability = null;
            this.Q(name: "Icon").style.backgroundImage = null;
        }

        [Preserve]
        public bool IsAssigned => Entity.Null != _owner && _ability != null;

        [Preserve]
        public void UpdateCoolDown()
        {
            if (!IsAssigned) return;
            (float, float) remainingTime = _entityManager.GetBuffer<AbilityBuffer>(_owner)[_index].Ability.CoolDown.ComputeRemainingTime();
            SetTime(remainingTime.Item1);
            SetBackGroundFill(remainingTime.Item2);
        }

        [Preserve]
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

        [Preserve]
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

        [Preserve]
        private void SetIcon(Texture2D icon)
        {
            this.Q(name = "Icon").style.backgroundImage = new StyleBackground(icon);
        }

    }
}
