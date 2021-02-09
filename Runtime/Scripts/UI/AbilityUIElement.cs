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
        public Entity CommandedEntity;
        public uint AbilityID;
        public EntityManager EntityManager;

        public void Execute()
        {
            if (!EntityManager.Exists(CommandedEntity) || !EntityManager.HasComponent<AbilityInput>(CommandedEntity)) return;
            EntityManager.SetComponentData(CommandedEntity, new AbilityInput(AbilityID));
        }
    }

    class AbilityUIElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AbilityUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private ICommand _command;
        private bool TryGetAbilityFromCatalogue(uint abilityId, out ScriptableAbility ability)
        {
            if (World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AddressableAbilityCatalogSystem>().AbilityCatalog.TryGetValue(_abilityId, out ability))
            {
                return true;
            }
            ability = null;
            return false;
        }

        private Entity _owner;
        private uint _abilityId;

        private Texture2D Icon;
        private float CooldownTime;
        private ScriptableAbility _ability;

        public AbilityUIElement()
        {

            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUIElement");
            visualTree.CloneTree(this);
            SetTime(0);
            SetBackGroundFill(0);
            _command = default;

            RegisterCallback<MouseEnterEvent>(e =>
            {
                VisualElement ve = (VisualElement)e.target;
                ve.panel.visualTree.Q<AbilityUITooltip>(name: "Tooltip").Show(_ability);
            });
            RegisterCallback<MouseLeaveEvent>(e =>
            {
                VisualElement ve = (VisualElement)e.target;
                ve.panel.visualTree.Q<AbilityUITooltip>(name: "Tooltip").Hide();
            });
        }

        public void AssignAbility(Entity owner, uint abilityID, EntityManager entityManager)
        {

            entityManager.World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate += UpdateCalatoguedInfo;
            _abilityId = abilityID;
            _owner = owner;

            _command = new AbilityUICommand()
            {
                CommandedEntity = owner,
                AbilityID = abilityID,
                EntityManager = entityManager
            };

            this.Q<Button>(name: "abilityButton").RegisterCallback<ClickEvent>(Clicked);

            UpdateCalatoguedInfo(null);
            UpdateCoolDown();
        }

        private void Clicked(ClickEvent evt)
        {
            Debug.Log("Clicked");
            _command.Execute();
        }

        private string Tooltip_Title = "";

        private void UpdateCalatoguedInfo(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {

            if (TryGetAbilityFromCatalogue(_abilityId, out ScriptableAbility ability))
            {
                SetIcon(ability.Icon);
                Tooltip_Title = ability.name;
                _ability = ability;
            }
        }

        public void UnassignAbility()
        {
            _command = default;
            SetIcon(null);
            Tooltip_Title = "";
            this.Q<Button>(name: "abilityButton").clicked -= _command.Execute;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate -= UpdateCalatoguedInfo;

        }

        public bool IsAssigned => !_command.Equals(default);

        public void UpdateCoolDown()
        {
            if (!IsAssigned) return;
            if (TryGetAbilityFromCatalogue(_abilityId, out ScriptableAbility ability)
                && TryFindAbilityInBuffer(_abilityId, out AbilityBufferElement bufferElement)
                && bufferElement.AbilityState == AbilityState.CoolingDown)
            {
                float remainingTime = bufferElement.CurrentTimming;
                SetTime(remainingTime);

                SetBackGroundFill((remainingTime / ability.Timings.CoolDown));
            }
        }


        private int _cachedAbilityIndex;

        private bool TryFindAbilityInBuffer(uint abilityId, out AbilityBufferElement ability)
        {
            ability = default;
            DynamicBuffer<AbilityBufferElement> buffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<AbilityBufferElement>(_owner);
            if (buffer[_cachedAbilityIndex].Guid == abilityId)
            {
                ability = buffer[_cachedAbilityIndex];
                return true;
            }
            else
            {
                for (int i = 0; i <= buffer.Length; ++i)
                {
                    if (buffer[i].Guid == abilityId)
                    {
                        ability = buffer[i];
                        _cachedAbilityIndex = i;
                        return true;
                    }
                }

            }
            return false;
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
            seconds = math.round(seconds * 10) / 10;
            label.text = seconds <= 0 ? "" : $"{seconds}s";
        }

        private void SetIcon(Texture2D icon)
        {
            this.Q(name: "Icon").style.backgroundImage = new StyleBackground(icon);
        }

    }
}
