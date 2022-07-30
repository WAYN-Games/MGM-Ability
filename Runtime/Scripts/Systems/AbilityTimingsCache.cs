using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public struct AbilityTimingsCache : ICacheComponent
    {
        #region Public Properties

        public BlobAssetReference<BlobMultiHashMap<uint, AbilityTimings>> Cache { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            if (Cache.IsCreated) Cache.Dispose();
        }

        #endregion Public Methods
    }

    public struct AbilitySpawnableCache : ICacheComponent
    {
        #region Public Properties

        public BlobAssetReference<BlobMultiHashMap<uint, Entity>> Cache { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            if (Cache.IsCreated) Cache.Dispose();
        }

        #endregion Public Methods
    }
}