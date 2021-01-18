using System.Collections.Generic;

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
        public Entity TargetEntity;
        public uint AbilityID;
        public EntityManager EntityManager;

        public void Execute()
        {
            if (!EntityManager.Exists(TargetEntity) || !EntityManager.HasComponent<AbilityInput>(TargetEntity)) return;
            EntityManager.SetComponentData(TargetEntity, new AbilityInput(AbilityID));
            Debug.Log($"Triggereing ability {AbilityID} for entity {TargetEntity}");
        }
    }

    class AbilityUIElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AbilityUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private ICommand _command;
        private Dictionary<uint, ScriptableAbility> _abilityCatalogue;

        private Entity _owner;
        private uint _abilityId;

        private Texture2D Icon;
        private float CooldownTime;

        public AbilityUIElement()
        {

            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUIElement");
            visualTree.CloneTree(this);
            SetTime(0);
            SetBackGroundFill(0);
            _command = default;
        }

        public void AssignAbility(Entity owner, uint abilityID, EntityManager entityManager)
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate += UpdateCalatoguedInfo;
            _command = new AbilityUICommand()
            {
                TargetEntity = owner,
                AbilityID = abilityID,
                EntityManager = entityManager
            };
            this.Q<Button>(name: "abilityButton").clicked += _command.Execute;
        }

        private void UpdateCalatoguedInfo(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {
            _abilityCatalogue = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AddressableAbilityCatalogSystem>().AbilityCatalog;


            if (_abilityCatalogue.TryGetValue(_abilityId, out ScriptableAbility ability))
            {
                this.Q(name: "Icon").style.backgroundImage = new StyleBackground(_abilityCatalogue[_abilityId].Icon);
            }

        }

        public void UnassignAbility()
        {
            _command = default;
            this.Q(name: "Icon").style.backgroundImage = null;
            this.Q<Button>(name: "abilityButton").clicked -= _command.Execute;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate -= UpdateCalatoguedInfo;

        }

        public bool IsAssigned => !_command.Equals(default);

        public void UpdateCoolDown()
        {
            if (!IsAssigned) return;
            float remainingTime = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<AbilityBufferElement>(_owner)[0].CurrentTimming;
            SetTime(remainingTime);
            SetBackGroundFill(1 - (remainingTime / _abilityCatalogue[_abilityId].Timings.CoolDown));
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
