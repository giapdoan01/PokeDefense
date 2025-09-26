using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropPokemon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject pokemonPrefab;  // prefab thật
    public GameObject ghostPrefab;    // prefab giả (trong suốt)

    private GameObject ghostInstance;
    private PlacementSlot currentSlot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        ghostInstance = Instantiate(ghostPrefab);
        ghostInstance.SetActive(false); // ban đầu chưa hiện
    }

    public void OnDrag(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlacementSlot slot = hit.collider.GetComponent<PlacementSlot>();

            if (slot != null && slot.CanPlace())
            {
                ghostInstance.SetActive(true);
                ghostInstance.transform.position = slot.placePoint ? slot.placePoint.position : slot.transform.position;
                currentSlot = slot;
            }
            else
            {
                ghostInstance.SetActive(false);
                currentSlot = null;
            }
        }
        else
        {
            ghostInstance.SetActive(false);
            currentSlot = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentSlot != null)
        {
            currentSlot.PlacePokemon(pokemonPrefab);
        }

        if (ghostInstance != null)
            Destroy(ghostInstance);
    }
}
