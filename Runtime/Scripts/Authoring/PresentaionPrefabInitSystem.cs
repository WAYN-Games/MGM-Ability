using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

public partial class PresentaionPrefabInitSystem : SystemBase
{
    #region Protected Methods

    protected override void OnUpdate()
    {
        Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity, PresentaionPrefab prefab) =>
        {
            GameObject Prefab = GameObject.Instantiate(prefab.prefab);


            // link GO and Enitty to make them share position.
            EntityManager.AddComponentObject(entity, Prefab.transform);
            EntityManager.AddComponent<CopyTransformToGameObject>(entity);

            // Give entity control over the GO's Animator
            Animator animator = Prefab.GetComponent<Animator>();
            EntityManager.AddComponentObject(entity, animator);

            // Give entity control over the GO's Animator
            VFXControler vfxControler = Prefab.GetComponent<VFXControler>();
            EntityManager.AddComponentObject(entity, vfxControler);

            // Mark entity as initised
            EntityManager.RemoveComponent<PresentaionPrefab>(entity);
        }).Run();
      
  
    }

    #endregion Protected Methods
}