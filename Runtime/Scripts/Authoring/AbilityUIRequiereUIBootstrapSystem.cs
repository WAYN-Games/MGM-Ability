using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.UI;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(AddressableAbilityCatalogSystem))]
public struct AbilitiesInitializationSystem : ISystemBase
{
    private EntityQuery _newEntityWithAbilities;
    private EntityQuery _cache;
    public void OnCreate(ref SystemState state)
    {
        _newEntityWithAbilities = state.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadOnly<AbilitiesMapIndex>() },
            None = new ComponentType[] { ComponentType.ReadOnly<AbilityCooldownBufferElement>() }
        });
        _cache = state.GetEntityQuery(ComponentType.ReadOnly(typeof(AbilityTimingsCache)));
        state.RequireSingletonForUpdate<AbilityTimingsCache>();
    }

    public void OnDestroy(ref SystemState state)
    {
        
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = state.World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>().CreateCommandBuffer();
        AbilityTimingsCache cache = _cache.GetSingleton<AbilityTimingsCache>();

        state.Dependency = new InitializeAbilityCooldownJob()
        {
            Cache = cache.Cache,
            AbilityMapChunk = state.GetComponentTypeHandle<AbilitiesMapIndex>(true),
            EntityChunk = state.GetEntityTypeHandle(),
            Ecb = ecb.AsParallelWriter()

        }.ScheduleSingle(_newEntityWithAbilities, state.Dependency);
        state.World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>().AddJobHandleForProducer(state.Dependency);

    }

    public struct InitializeAbilityCooldownJob : IJobChunk
    {
        [ReadOnly] public BlobAssetReference<BlobMultiHashMap<uint, AbilityTimings>> Cache;
        [ReadOnly] public ComponentTypeHandle<AbilitiesMapIndex> AbilityMapChunk;
        [ReadOnly] public EntityTypeHandle EntityChunk;

        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AbilitiesMapIndex> inputArray = chunk.GetNativeArray(AbilityMapChunk);
            NativeArray<Entity> entitiesArray = chunk.GetNativeArray(EntityChunk);

            ref var cahcheMap = ref Cache.Value;

            for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
            {
                AbilitiesMapIndex entityAbilities = inputArray[entityIndex];
                ref BlobMultiHashMap<int, uint> indexToGuid = ref entityAbilities.indexToGuid.Value;

                int abilityCount = indexToGuid.ValueCount.Value;

                NativeArray<AbilityCooldownBufferElement> abilityBuffer = new NativeArray<AbilityCooldownBufferElement>(abilityCount, Allocator.Temp);
                for(int i = 0; i < abilityCount; i++)
                {

                    abilityBuffer[i] = new AbilityCooldownBufferElement()
                    {
                        CooldownTime = 2 // cahcheMap.GetValuesForKey(indexToGuid.GetValuesForKey(i)[0])[0].CoolDown
                    };
                }


                DynamicBuffer< AbilityCooldownBufferElement> buffer = Ecb.AddBuffer<AbilityCooldownBufferElement>(entityIndex, entitiesArray[entityIndex]);
                buffer.AddRange(abilityBuffer);

                Ecb.AddComponent<CurrentlyCasting>(entityIndex, entitiesArray[entityIndex], new CurrentlyCasting() { castTime = float.NaN });
                Ecb.AddComponent<AbilityInput>(entityIndex, entitiesArray[entityIndex],new AbilityInput(indexToGuid.GetValuesForKey(0)[0]));
            }
        }
    }

}




class AbilityUIRequiereUIBootstrapSystem : SystemBase
    {
        Dictionary<uint, AbilityUiLink> uiMap;

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
            EntityManager.World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnAbilityUpdate += BootstrapUi;

        }

        private void BootstrapUi(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {
            LinkUiToEntity();
        }

        public void LinkUiToEntity()
        {
            Addressables.LoadAssetsAsync<AbilityUiLink>(new AssetLabelReference()
            {
                labelString = AbilityHelper.ADDRESSABLE_UiLink_LABEL
            }, null, false).Completed += objects =>
            {
                if (objects.Result == null) return;
                uiMap = new Dictionary<uint, AbilityUiLink>();
                foreach (AbilityUiLink uiLink in objects.Result)
                {
                    uiMap.Add(uiLink.Id, uiLink);
                }

                Enabled = true;
            }; ;
        }

        protected override void OnUpdate()
        {

            Entities.WithStructuralChanges().ForEach((Entity entity, ref RequiereUIBootstrap boostrap) =>
            {

                if (uiMap.TryGetValue(boostrap.uiAssetGuid, out var link))
                {
                    UIDocument uiDoc = Object.Instantiate(link.UiPrefab).GetComponent<UIDocument>();

                    if (uiDoc != null)
                    {
                        uiDoc.panelSettings = link.PanelSettings;
                        uiDoc.visualTreeAsset = link.UxmlUi;
                        uiDoc.sortingOrder = link.SortingOrder;
                        AbilityBookUIElement book = uiDoc.rootVisualElement.Q<AbilityBookUIElement>();
                        if (book != null) book.Populate(entity, EntityManager);
                        CastBar CastBar = uiDoc.rootVisualElement.Q<CastBar>();
                        if (CastBar != null) CastBar.SetOwnership(entity, EntityManager);
                    }
                }
                EntityManager.RemoveComponent<RequiereUIBootstrap>(entity);
            }).WithoutBurst().Run();
        }


    }
