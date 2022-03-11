using Unity.Entities;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{
    internal abstract class EntityOwnedUpdatableVisualElement : VisualElement
    {
        #region Protected Fields

        protected Entity _owner;
        protected EntityManager _entityManager;

        #endregion Protected Fields

        #region Public Constructors

        public EntityOwnedUpdatableVisualElement()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public abstract void Update();

        public void SetOwnership(Entity owner, EntityManager entityManager)
        {
            _owner = owner;
            _entityManager = entityManager;
        }

        #endregion Public Methods
    }
}