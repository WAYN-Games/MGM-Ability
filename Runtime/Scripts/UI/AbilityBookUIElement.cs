using Unity.Entities;

using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{
    internal class AbilityBookUIElement : VisualElement
    {
        #region Private Fields

        private VisualElement _bookRoot;

        #endregion Private Fields

        #region Public Constructors

        public AbilityBookUIElement()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("AbilityBookUIElement");
            visualTree.CloneTree(this);
            _bookRoot = this;
        }

        #endregion Public Constructors

        #region Public Methods

        public void Populate(Entity owner, EntityManager entityManager)
        {
            _bookRoot.Clear();
            var abilitiesMapIndex = entityManager.GetComponentData<AbilitiesMapIndex>(owner);

            ref BlobMultiHashMap<int, uint> indexToGuid = ref abilitiesMapIndex.indexToGuid.Value;

            int abilityCount = indexToGuid.ValueCount.Value;
            for (int i = 0; i < abilityCount; i++)
            {
                AbilityUIElement uiAbility = new AbilityUIElement();
                var array = indexToGuid.GetValuesForKey(i);
                if (!array.IsCreated)
                {
                    Debug.Log("Fail");
                    continue;
                }
                uint abilityId = array[0];
                uiAbility.AssignAbility(owner, abilityId, entityManager);
                _bookRoot.Add(uiAbility);
            }
        }

        #endregion Public Methods

        #region Public Classes

        public new class UxmlFactory : UxmlFactory<AbilityBookUIElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        { }

        #endregion Public Classes
    }
}