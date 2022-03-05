using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using WaynGroup.Mgm.Ability;

[DisallowMultipleComponent]
public partial class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<ScriptableAbility> Abilities;

    [Tooltip("Optional : Data bout the UI that control this entity.")]
    public AbilityUiLink AbilityUiLink;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Whatever the authoring order if 2 entities have the same set of abilities,
        // ordering them makes sure that we won't duplciate the blobasset refrence
        // and that both entites will have the same index for the same abilities.
        Abilities.Sort((x, y) => x.Id.CompareTo(y.Id));
        dstManager.AddComponentData(entity, CreateAbilitiesBlobMap(conversionSystem));

        LinkUI(entity, dstManager, conversionSystem);

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

    /// <summary>
    /// Create a Blob Map of abilities to easily reference the index of an ability in a buffer from it's guid.
    /// </summary>
    /// <param name="conversionSystem"> Used to ensure blob unicity and asset dependancy.</param>
    /// <returns>A component containg the blob map asset reference.</returns>
    private AbilitiesMapIndex CreateAbilitiesBlobMap(GameObjectConversionSystem conversionSystem)
    {

        AbilitiesMapIndex AbilitiesMapIndex = new AbilitiesMapIndex();

        using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
        {
            var guidIndexMapBuilder = new BlobHashMapBuilder<uint, int>(bb);
                for (int i = 0; i < Abilities.Count; i++)
                {
                    guidIndexMapBuilder.Add(Abilities[i].Id, i);
#if UNITY_EDITOR
                    // Declares a dependancy on hte ability asset so that if it is changed,
                    // the entity will be reconverted to take the changed int account
                    // both in subscenes and play mode.
                    conversionSystem.DeclareAssetDependency(gameObject, Abilities[i]);
#endif
                }
            BlobAssetReference<BlobMultiHashMap<uint, int>> guidToIndex = guidIndexMapBuilder.CreateBlobAssetReference(Allocator.Persistent);
                conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref guidToIndex);
                AbilitiesMapIndex.guidToIndex = guidToIndex;
        }

        using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
        {

            var indexToGuidMapBuilder = new BlobHashMapBuilder<int, uint>(bb);
                for (int i = 0; i < Abilities.Count; i++)
                {
                    indexToGuidMapBuilder.Add(i, Abilities[i].Id);
#if UNITY_EDITOR
                    // Declares a dependancy on hte ability asset so that if it is changed,
                    // the entity will be reconverted to take the changed int account
                    // both in subscenes and play mode.
                    conversionSystem.DeclareAssetDependency(gameObject, Abilities[i]);
#endif
                }
            BlobAssetReference<BlobMultiHashMap<int, uint>> indexToGuid = indexToGuidMapBuilder.CreateBlobAssetReference(Allocator.Persistent);

                conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref indexToGuid);
                AbilitiesMapIndex.indexToGuid = indexToGuid;

        }

        return AbilitiesMapIndex;
    }


}


public struct AbilitiesMapIndex : IComponentData
{
    public BlobAssetReference<BlobMultiHashMap<uint, int>> guidToIndex;
    public BlobAssetReference<BlobMultiHashMap<int, uint>> indexToGuid;
}


public struct RequiereUIBootstrap : IComponentData
{
    public uint uiAssetGuid;
}
