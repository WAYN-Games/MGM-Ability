using UnityEngine;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability.UI;

[RequireComponent(typeof(UIDocument))]
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
        _uiDocument.rootVisualElement.Query<EntityOwnedUpdatableVisualElement>().ForEach((EntityOwnedUpdatableVisualElement ve) => { ve.Update(); });
    }

}
