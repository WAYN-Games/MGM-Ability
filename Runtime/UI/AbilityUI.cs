using Unity.Entities;

using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AbilityUI : MonoBehaviour
{
    private VisualElement _root;

    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
    }

    public void AssignAbility(Entity owner, int index, ScriptableAbility ability, int bar, int slot, EntityManager entityManager)
    {
        //AbilityUIElement abilityUIElement = ((VisualElement)_root.Query(name: $"Bar{bar}")).Query<AbilityUIElement>(name: $"Slot{slot}");
        //abilityUIElement.AssignAbility(owner, index, ability, entityManager);
    }

    void Update()
    {
        // _root.Query<AbilityUIElement>().ForEach((AbilityUIElement abilityUIElement) => { abilityUIElement.UpdateCoolDown(); });
    }


}
