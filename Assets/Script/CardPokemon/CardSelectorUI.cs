using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSelectorUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image cardImage;
    public TMP_Text cardNameText;
    public Button selectButton;
    public GameObject selectedFrame; // Khung viền khi được chọn
    public GameObject selectButtonGameObject; // Đối tượng nút chọn để ẩn/hiện
    
    private CardData cardData;
    private int targetSlotIndex;
    
    public void Setup(CardData card, int slotIndex)
    {
        cardData = card;
        targetSlotIndex = slotIndex;
        
        // Hiển thị thông tin thẻ
        if (cardImage != null) 
            cardImage.sprite = card.cardImage;
        
        if (cardNameText != null) 
            cardNameText.text = card.name;
        
        // Gắn sự kiện chọn thẻ
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectClicked);
        }
        
        UpdateSelectedState();
    }
    
    public void UpdateSelectedState()
    {
        if (PlayerDataManager.Instance == null || cardData == null)
        {
            SetSelected(false);
            return;
        }
        
        var cardDeck = PlayerDataManager.Instance.currentPlayerData.cardDeck;
        
        // Kiểm tra thẻ này có đang được chọn ở slot nào không
        bool isSelected = cardDeck.Contains(cardData.id);
        SetSelected(isSelected);
    }
    
    void SetSelected(bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
            selectButtonGameObject.SetActive(!selected); // Ẩn nút chọn nếu đã được chọn
        }
    }
    
    void OnSelectClicked()
    {
        if (DeckManager.Instance != null && cardData != null)
        {
            DeckManager.Instance.SelectCardForSlot(targetSlotIndex, cardData.id);
        }
    }
}
