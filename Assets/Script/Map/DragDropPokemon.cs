using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropPokemon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject pokemonPrefab;
    public GameObject ghostPrefab;

    private GameObject ghostInstance;
    private PlacementSlot currentSlot;
    private CardData cardData;

    // Phương thức để nhận CardData từ DeckCardUIInBattle
    public void SetCardData(CardData data)
    {
        cardData = data;
        if (data == null)
        {
            Debug.LogError("SetCardData được gọi với data là null!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData == null)
        {
            Debug.LogError("CardData là null! Hãy đảm bảo đã gọi SetCardData() trước khi kéo.");
            return;
        }

        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats.Instance là null!");
            return;
        }

        if (PlayerStats.Instance.coin < cardData.coinInGame)
        {
            Debug.LogWarning($"Không đủ tiền! Cần: {cardData.coinInGame}, Có: {PlayerStats.Instance.coin}");
            return;
        }

        ghostInstance = Instantiate(ghostPrefab);
        ghostInstance.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostInstance == null) return;

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
        if (ghostInstance == null) return;

        if (currentSlot != null && cardData != null && PlayerStats.Instance != null)
        {
            if (PlayerStats.Instance.coin >= cardData.coinInGame)
            {
                PlayerStats.Instance.SpendCoin(cardData.coinInGame);
                
                currentSlot.PlacePokemon(pokemonPrefab);
                
                Debug.Log($"Đã tạo {cardData.name} với giá {cardData.coinInGame}. Số tiền còn lại: {PlayerStats.Instance.coin}");
            }
            else
            {
                Debug.LogWarning($"Không đủ tiền để tạo Pokemon! Cần: {cardData.coinInGame}, Có: {PlayerStats.Instance.coin}");
            }
        }
        if (ghostInstance != null)
            Destroy(ghostInstance);
    }
}