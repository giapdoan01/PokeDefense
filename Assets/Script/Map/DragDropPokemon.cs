using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropPokemon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject pokemonPrefab;  // prefab thật
    public GameObject ghostPrefab;    // prefab giả (trong suốt)

    private GameObject ghostInstance;
    private PlacementSlot currentSlot;
    private CardData cardData;  // Thêm biến CardData

    // Phương thức để nhận CardData từ DeckCardUIInBattle
    public void SetCardData(CardData data)
    {
        cardData = data;
        if (data == null)
        {
            Debug.LogError("❌ SetCardData được gọi với data là null!");
        }
        else
        {
            Debug.Log($"✅ Đã gán CardData: {data.name}, Giá: {data.coinInGame}");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Kiểm tra CardData và tiền
        if (cardData == null)
        {
            Debug.LogError("❌ CardData là null! Hãy đảm bảo đã gọi SetCardData() trước khi kéo.");
            return;
        }

        // Kiểm tra có đủ tiền không
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("❌ PlayerStats.Instance là null!");
            return;
        }

        if (PlayerStats.Instance.coin < cardData.coinInGame)
        {
            Debug.LogWarning($"⚠️ Không đủ tiền! Cần: {cardData.coinInGame}, Có: {PlayerStats.Instance.coin}");
            return;
        }

        // Tạo ghost prefab
        ghostInstance = Instantiate(ghostPrefab);
        ghostInstance.SetActive(false); // ban đầu chưa hiện
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Nếu chưa có ghost instance (có thể do không đủ tiền), dừng xử lý
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
            // Kiểm tra lại tiền (phòng trường hợp thay đổi trong quá trình kéo)
            if (PlayerStats.Instance.coin >= cardData.coinInGame)
            {
                // Trừ tiền
                PlayerStats.Instance.SpendCoin(cardData.coinInGame);
                
                // Spawn Pokemon
                currentSlot.PlacePokemon(pokemonPrefab);
                
                Debug.Log($"✅ Đã tạo {cardData.name} với giá {cardData.coinInGame}. Số tiền còn lại: {PlayerStats.Instance.coin}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Không đủ tiền để tạo Pokemon! Cần: {cardData.coinInGame}, Có: {PlayerStats.Instance.coin}");
            }
        }

        // Xóa ghost dù có tạo Pokemon thành công hay không
        if (ghostInstance != null)
            Destroy(ghostInstance);
    }
}