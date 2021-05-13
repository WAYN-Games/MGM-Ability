using Unity.Entities;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{
    abstract class EntityOwnedUpdatableVisualElement : VisualElement
    {
        protected Entity _owner;
        protected EntityManager _entityManager;

        public EntityOwnedUpdatableVisualElement()
        {

        }

        public abstract void Update();
        public void SetOwnership(Entity owner, EntityManager entityManager)
        {
            _owner = owner;
            _entityManager = entityManager;
        }

    }



}
