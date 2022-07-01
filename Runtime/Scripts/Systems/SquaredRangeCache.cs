using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public struct SquaredRangeCache : ICacheComponent
    {
        #region Public Properties

        public BlobAssetReference<BlobMultiHashMap<uint, Range>> Cache { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            if (Cache.IsCreated) Cache.Dispose();
        }

        #endregion Public Methods
    }
}