using Unity.Entities;

public struct AbilitiesMapIndex : IComponentData
{
    #region Public Fields

    public BlobAssetReference<BlobMultiHashMap<uint, int>> guidToIndex;
    public BlobAssetReference<BlobMultiHashMap<int, uint>> indexToGuid;

    #endregion Public Fields
}