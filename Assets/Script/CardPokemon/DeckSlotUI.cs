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
        
        if (changeCardButton != null)
        {
            changeCardButton.onClick.RemoveAllListeners();
            changeCardButton.onClick.AddListener(OnChangeCardClicked);
        }
        
        HideSelectionFrame();
        
        RefreshDisplay();
    }
    
    public void RefreshDisplay()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentPlayerData == null)
        {
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

        if (slotIndex < cardDeck.Count && !string.IsNullOrEmpty(cardDeck[slotIndex]))
        {
            currentCardId = cardDeck[slotIndex];
            ShowCard(currentCardId);
        }
        else
        {
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
            cardImage.sprite = emptyCardSprite; 
        }
    }
    
    void OnChangeCardClicked()
    {
        if (DeckManager.Instance != null)
        {
            ShowSelectionFrame();
            DeckManager.Instance.OpenCardSelector(slotIndex);
        }
    }
    
    public void ShowSelectionFrame()
    {
        if (cardFrame != null)
            cardFrame.SetActive(true);
    }
    
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