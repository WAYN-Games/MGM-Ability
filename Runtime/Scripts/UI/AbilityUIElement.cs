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

    class AbilityUIElement : EntityOwnedUpdatableVisualElement
    {
        public new class UxmlFactory : UxmlFactory<AbilityUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private ICommand _command;


        private Texture2D Icon;
        private ScriptableAbility _ability;
        private ProgressBar _CoolDown;

        public AbilityUIElement()
        {

            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityUIElement");
            visualTree.CloneTree(this);
            VisualElement root = this.Q<VisualElement>(name: "Shape");
            _CoolDown = this.Q<ProgressBar>(name: "coolDown");
            _CoolDown.Value = 0;
            _CoolDown.SetOrientation(FlexDirection.ColumnReverse);
            _command = default;
            this.AddManipulator(new DragableAbility());
        }

        public AbilityUIElement Clone()
        {
            AbilityUIElement copy = new AbilityUIElement();
            copy.AssignAbility(_owner, _ability.Id, _entityManager);
            return copy;
        }

        public void AssignAbility(Entity owner, uint abilityID, EntityManager entityManager)
        {
            Debug.Log($"Assigning ability {abilityID}.");
            _entityManager = entityManager;
            entityManager.World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate += UpdateCalatoguedInfo;

            _owner = owner;

            _command = new AbilityUICommand()
            {
                CommandedEntity = owner,
                AbilityID = abilityID,
                EntityManager = entityManager
            };


            // Need to force the first update on assignement because the ability assignement happens after the first catalogue update
            UpdatedCachedInfo(entityManager.World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().AbilityCatalog, abilityID);


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

        public void ExecuteAction()
        {
            _command.Execute();
        }

        private void UpdateCalatoguedInfo(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {
            UpdatedCachedInfo(abilityCatalogue, _ability.Id);
        }

        private void UpdatedCachedInfo(Dictionary<uint, ScriptableAbility> abilityCatalogue, uint abilityID)
        {
            if (abilityCatalogue.TryGetValue(abilityID, out ScriptableAbility ability))
            {
                SetIcon(ability.Icon);
                _ability = ability;
                UpdateCoolDown();
            }
        }

        public void UnassignAbility()
        {
            _command = default;
            SetIcon(null);

            this.Q<Button>(name: "abilityButton").clicked -= _command.Execute;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate -= UpdateCalatoguedInfo;

        }

        public bool IsAssigned => !_command.Equals(default);

        public void UpdateCoolDown()
        {

            if (_ability == null) return;
            if (!IsAssigned) return;

            if (
                TryFindAbilityInBuffer(_ability.Id, out AbilityBufferElement bufferElement)
                && bufferElement.AbilityState == AbilityState.CoolingDown)
            {
                float remainingTime = bufferElement.CurrentTimming;
                _CoolDown.Value = remainingTime / _ability.Timings.CoolDown * 100;
                _CoolDown.Title = $"{math.round(remainingTime / .1f) * .1f} s";
            }
            else
            {
                _CoolDown.Value = 0;
                _CoolDown.Title = $"";
            }
        }


        private int _cachedAbilityIndex;

        private bool TryFindAbilityInBuffer(uint abilityId, out AbilityBufferElement ability)
        {
            ability = default;
            DynamicBuffer<AbilityBufferElement> buffer = _entityManager.GetBuffer<AbilityBufferElement>(_owner);
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

        private void SetIcon(Texture2D icon)
        {
            this.Q(name: "Icon").style.backgroundImage = new StyleBackground(icon);
        }

        public override void Update()
        {
            UpdateCoolDown();
        }
    }



}
