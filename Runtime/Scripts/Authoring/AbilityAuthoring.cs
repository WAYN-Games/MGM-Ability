using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public partial class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    #region Public Fields

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<ScriptableAbility> Abilities;

    [Tooltip("Optional : Data bout the UI that control this entity.")]
    public AbilityUiLink AbilityUiLink;

    public Animator Animator;

    public GameObject PresentationPrefab;

    #endregion Public Fields

    #region Public Methods

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Whatever the authoring order if 2 entities have the same set of abilities, ordering them
        // makes sure that we won't duplciate the blobasset refrence and that both entites will have
        // the same index for the same abilities.
        dstManager.AddComponentData(entity, CreateAbilitiesBlobMap(conversionSystem));

        LinkUI(entity, dstManager, conversionSystem);
        dstManager.AddComponentData(entity, new PresentaionPrefab() { prefab = PresentationPrefab });
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Create a Blob Map of abilities to easily reference the index of an ability in a buffer from
    /// it's guid.
    /// </summary>
    /// <param name="conversionSystem">Used to ensure blob unicity and asset dependancy.</param>
    /// <returns>A component containg the blob map asset reference.</returns>
    private AbilitiesMapIndex CreateAbilitiesBlobMap(GameObjectConversionSystem conversionSystem)
    {
        AbilitiesMapIndex AbilitiesMapIndex = new AbilitiesMapIndex();
        // Ordering abilities to generate the same blobasset for the same set of ability whatever
        // the authoring order. Can't sort the array in place or it will affect the authoring order.
        List<ScriptableAbility> orederedList = Abilities.OrderBy(item => item.Id).ToList();
        using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
        {
            var guidIndexMapBuilder = new BlobHashMapBuilder<uint, int>(bb);
            for (int i = 0; i < orederedList.Count; i++)
            {
                guidIndexMapBuilder.Add(orederedList[i].Id, i);
#if UNITY_EDITOR
                // Declares a dependancy on hte ability asset so that if it is changed, the entity
                // will be reconverted to take the changed int account both in subscenes and play mode.
                conversionSystem.DeclareAssetDependency(gameObject, orederedList[i]);
#endif
            }
            BlobAssetReference<BlobMultiHashMap<uint, int>> guidToIndex = guidIndexMapBuilder.CreateBlobAssetReference(Allocator.Persistent);
            conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref guidToIndex);
            AbilitiesMapIndex.guidToIndex = guidToIndex;
        }

        using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
        {
            var indexToGuidMapBuilder = new BlobHashMapBuilder<int, uint>(bb);
            for (int i = 0; i < orederedList.Count; i++)
            {
                indexToGuidMapBuilder.Add(i, orederedList[i].Id);
            }
            BlobAssetReference<BlobMultiHashMap<int, uint>> indexToGuid = indexToGuidMapBuilder.CreateBlobAssetReference(Allocator.Persistent);

            conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref indexToGuid);
            AbilitiesMapIndex.indexToGuid = indexToGuid;
        }

        return AbilitiesMapIndex;
    }

    /// <summary>
    /// Add a reference to the UI associated with thi entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dstManager"></param>
    /// <param name="conversionSystem">Used to ensure asset dependancy.</param>
    private void LinkUI(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (AbilityUiLink != null)
        {
#if UNITY_EDITOR
            conversionSystem.DeclareAssetDependency(gameObject, AbilityUiLink);
#endif
            dstManager.AddComponentData(entity, new RequiereUIBootstrap()
            {
                uiAssetGuid = AbilityUiLink.Id
            });
        }
    }

    #endregion Private Methods
}