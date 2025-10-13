using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image cardImage;
    public Button changeCardButton;
    public GameObject cardFrame; 
    
    [Header("Empty State")]
    public Sprite emptyCardSprite;
    
    private int slotIndex;
    private string currentCardId;
    
    public void Setup(int index)
    {
        slotIndex = index;
        
        // Gắn sự kiện nút đổi thẻ
        if (changeCardButton != null)
        {
            changeCardButton.onClick.RemoveAllListeners();
            changeCardButton.onClick.AddListener(OnChangeCardClicked);
        }
        
        // Ẩn frame khi khởi tạo
        HideSelectionFrame();
        
        RefreshDisplay();
    }
    
    public void RefreshDisplay()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentPlayerData == null)
        {
            Debug.Log($"Slot {slotIndex}: PlayerDataManager null hoặc chưa có data");
            ShowEmpty();
            return;
        }
        
        var cardDeck = PlayerDataManager.Instance.currentPlayerData.cardDeck;
        
        if (cardDeck == null)
        {
            Debug.LogWarning($"Slot {slotIndex}: cardDeck là null");
            ShowEmpty();
            return;
        }
        
        // Log để debug
        Debug.Log($"Slot {slotIndex}: Card Deck có {cardDeck.Count} thẻ");
        
        // Kiểm tra slot có thẻ không
        if (slotIndex < cardDeck.Count && !string.IsNullOrEmpty(cardDeck[slotIndex]))
        {
            currentCardId = cardDeck[slotIndex];
            Debug.Log($"Slot {slotIndex}: Đã tìm thấy thẻ {currentCardId}");
            ShowCard(currentCardId);
        }
        else
        {
            Debug.Log($"Slot {slotIndex}: Không tìm thấy thẻ hoặc slot >= cardDeck.Count");
            if (slotIndex >= cardDeck.Count)
            {
                Debug.Log($"slotIndex ({slotIndex}) >= cardDeck.Count ({cardDeck.Count})");
            }
            else if (string.IsNullOrEmpty(cardDeck[slotIndex]))
            {
                Debug.Log($"cardDeck[{slotIndex}] là null hoặc rỗng");
            }
            
            currentCardId = null;
            ShowEmpty();
        }
    }
    
    void ShowCard(string cardId)
    {
        if (CardManager.Instance == null)
        {
            Debug.LogError($"Slot {slotIndex}: CardManager.Instance là null");
            ShowEmpty();
            return;
        }
        
        CardData card = CardManager.Instance.GetCardById(cardId);
        
        if (card != null)
        {
            Debug.Log($"Slot {slotIndex}: Hiển thị thẻ {cardId} với hình ảnh {card.cardImage?.name ?? "null"}");
            if (cardImage != null && card.cardImage != null) 
            {
                cardImage.sprite = card.cardImage;
            }
            else
            {
                Debug.LogError($"Slot {slotIndex}: cardImage hoặc card.cardImage là null");
                ShowEmpty();
            }
        }
        else
        {
            Debug.LogWarning($"Slot {slotIndex}: Không tìm thấy CardData cho id {cardId}");
            ShowEmpty();
        }
    }
    
    void ShowEmpty()
    {
        if (cardImage != null) 
        {
            Debug.Log($"Slot {slotIndex}: Hiển thị trạng thái rỗng");
            cardImage.sprite = emptyCardSprite; 
        }
    }
    
    void OnChangeCardClicked()
    {
        if (DeckManager.Instance != null)
        {
            // Hiển thị frame khi slot được chọn để thay đổi
            ShowSelectionFrame();
            DeckManager.Instance.OpenCardSelector(slotIndex);
        }
    }
    
    // Hiện frame khi slot được chọn
    public void ShowSelectionFrame()
    {
        if (cardFrame != null)
            cardFrame.SetActive(true);
    }
    
    // Ẩn frame khi slot không được chọn
    public void HideSelectionFrame()
    {
        if (cardFrame != null)
            cardFrame.SetActive(false);
    }
    
    public string GetCurrentCardId()
    {
        return currentCardId;
    }
}