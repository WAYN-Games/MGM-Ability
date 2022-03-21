using UnityEngine;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability.UI;

[RequireComponent(typeof(UIDocument))]
public class UpdateUIManager : MonoBehaviour
{
    #region Private Fields

    private UIDocument _uiDocument;

    #endregion Private Fields

    #region Private Methods

    // Start is called before the first frame update
    private void Start()
    {
        _uiDocument = FindObjectOfType<UIDocument>(true);
    }

    // Update is called once per frame
    private void Update()
    {
        _uiDocument.rootVisualElement.Query<EntityOwnedUpdatableVisualElement>().ForEach((EntityOwnedUpdatableVisualElement ve) => { ve.Update(); });
    }

    #endregion Private Methods
}