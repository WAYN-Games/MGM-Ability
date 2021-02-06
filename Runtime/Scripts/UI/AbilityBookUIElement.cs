
using Unity.Entities;

using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{

    class AbilityBookUIElement : VisualElement
    {
        private VisualElement _bookRoot;

        public new class UxmlFactory : UxmlFactory<AbilityBookUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public AbilityBookUIElement()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityBookUIElement");
            visualTree.CloneTree(this);
            _bookRoot = this;
        }

        public void Populate(DynamicBuffer<AbilityBufferElement> abilities, Entity owner, EntityManager entityManager)
        {
            foreach (AbilityBufferElement ability in abilities)
            {
                AbilityUIElement uiAbility = new AbilityUIElement();
                uiAbility.AssignAbility(owner, ability.Guid, entityManager);
                _bookRoot.Add(uiAbility);
            }
        }
    }
}
