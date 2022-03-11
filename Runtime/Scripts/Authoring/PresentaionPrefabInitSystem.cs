using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class PresentaionPrefabInitSystem : SystemBase
{
    #region Protected Methods

    protected override void OnUpdate()
    {
        Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity, PresentaionPrefab prefab) =>
        {
            GameObject Prefab = GameObject.Instantiate(prefab.prefab);
            Animator animator = Prefab.GetComponent<Animator>();
            EntityManager.AddComponentObject(entity, animator);
            EntityManager.RemoveComponent<PresentaionPrefab>(entity);
        }).Run();

        Entities.WithoutBurst().ForEach((Entity entity, Animator animator, in LocalToWorld ltw) =>
        {
            animator.gameObject.transform.position = ltw.Position;
            animator.gameObject.transform.rotation = ltw.Rotation;
        }).Run();
    }

    #endregion Protected Methods
}