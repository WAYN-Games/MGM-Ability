using UnityEngine;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability.UI;

public class UpdateUIManager : MonoBehaviour
{
    UIDocument _uiDocument;
    // Start is called before the first frame update
    void Start()
    {
        _uiDocument = FindObjectOfType<UIDocument>();
    }

    // Update is called once per frame
    void Update()
    {
        _uiDocument.rootVisualElement.Query<AbilityUIElement>().ForEach((AbilityUIElement abilityUIElement) => { abilityUIElement.UpdateCoolDown(); });
    }
}
