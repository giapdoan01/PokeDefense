using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    
    [Header("Deck Slots")]
    public Transform deckSlotsContainer;
    public GameObject deckSlotPrefab;
    public int maxDeckSize = 4;
    
    [Header("Card Selector Panel")]
    public GameObject cardSelectorPanel;
    public Transform cardSelectorContainer;
    public GameObject cardSelectorItemPrefab;
    public Button closeSelectorButton;
    
    private List<DeckSlotUI> deckSlots = new List<DeckSlotUI>();
    private List<CardSelectorUI> cardSelectorItems = new List<CardSelectorUI>();
    private int currentEditingSlot = -1;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeDeckSlots();
        
        if (closeSelectorButton != null)
        {
            closeSelectorButton.onClick.AddListener(CloseCardSelector);
        }
        
        if (cardSelectorPanel != null)
        {
            cardSelectorPanel.SetActive(false);
        }
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnCardChanged += OnCardChanged;
            PlayerDataManager.Instance.OnDeckChanged += OnDeckChanged;
            PlayerDataManager.Instance.OnPlayerDataLoaded += OnPlayerDataLoaded;
        }
        
        if (CardManager.Instance != null)
        {
            Debug.Log("CardManager is ready");
        }
        else
        {
            Debug.LogWarning("CardManager is not ready yet!");
        }
        
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentPlayerData != null)
        {
            RefreshAllSlots();
        }
    }
    
    void OnPlayerDataLoaded(PlayerData data)
    {
        Debug.Log("Player data loaded, refreshing deck UI");
        RefreshAllSlots();
    }
    
    void OnDeckChanged()
    {
        Debug.Log("Deck changed, refreshing UI");
        RefreshAllSlots();
    }
    
    void InitializeDeckSlots()
    {
        foreach (Transform child in deckSlotsContainer)
        {
            Destroy(child.gameObject);
        }
        deckSlots.Clear();
        
        for (int i = 0; i < maxDeckSize; i++)
        {
            GameObject slotObj = Instantiate(deckSlotPrefab, deckSlotsContainer);
            DeckSlotUI slotUI = slotObj.GetComponent<DeckSlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Setup(i);
                deckSlots.Add(slotUI);
            }
        }
        
        Debug.Log($"Created {deckSlots.Count} deck slots");
    }
    
    public void OpenCardSelector(int slotIndex)
    {
        currentEditingSlot = slotIndex;

        HideAllSelectionFrames();
        
        if (slotIndex >= 0 && slotIndex < deckSlots.Count)
        {
            deckSlots[slotIndex].ShowSelectionFrame();
        }

        if (cardSelectorPanel != null)
        {
            cardSelectorPanel.SetActive(true);
        }
        
        LoadOwnedCards();
        
        Debug.Log($"Opening card selector for slot {slotIndex}");
    }
    
    public void CloseCardSelector()
    {
        if (cardSelectorPanel != null)
        {
            cardSelectorPanel.SetActive(false);
        }
        
        HideAllSelectionFrames();
        
        currentEditingSlot = -1;
        
        Debug.Log($"Closed card selector");
    }
    
    private void HideAllSelectionFrames()
    {
        foreach (DeckSlotUI slot in deckSlots)
        {
            slot.HideSelectionFrame();
        }
    }
    
    void LoadOwnedCards()
    {
        if (PlayerDataManager.Instance == null || CardManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager or CardManager not found!");
            return;
        }
        
        foreach (Transform child in cardSelectorContainer)
        {
            Destroy(child.gameObject);
        }
        cardSelectorItems.Clear();
        
        var ownedCardIds = PlayerDataManager.Instance.currentPlayerData.ownedCardIds;
        
        if (ownedCardIds == null || ownedCardIds.Count == 0)
        {
            Debug.LogWarning("No owned cards!");
            return;
        }
        
        foreach (string cardId in ownedCardIds)
        {
            CardData card = CardManager.Instance.GetCardById(cardId);
            
            if (card != null)
            {
                GameObject itemObj = Instantiate(cardSelectorItemPrefab, cardSelectorContainer);
                CardSelectorUI itemUI = itemObj.GetComponent<CardSelectorUI>();
                
                if (itemUI != null)
                {
                    itemUI.Setup(card, currentEditingSlot);
                    cardSelectorItems.Add(itemUI);
                }
            }
            else
            {
                Debug.LogWarning($"Cannot find card with ID: {cardId} in CardManager");
            }
        }
        
        Debug.Log($"Loaded {cardSelectorItems.Count} owned cards");
    }
    
    public void SelectCardForSlot(int slotIndex, string cardId)
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentPlayerData == null)
        {
            Debug.LogError("PlayerDataManager not found!");
            return;
        }
        
        var cardDeck = PlayerDataManager.Instance.currentPlayerData.cardDeck;
        
        if (cardDeck == null)
        {
            cardDeck = new List<string>();
            PlayerDataManager.Instance.currentPlayerData.cardDeck = cardDeck;
        }
        
        while (cardDeck.Count < maxDeckSize)
        {
            cardDeck.Add(null);
        }
        
        cardDeck[slotIndex] = cardId;
        
        PlayerDataManager.Instance.currentPlayerData.cardDeck = cardDeck;
        SaveDeck();
        
        RefreshAllSlots();
        RefreshCardSelector();
        
        Debug.Log($"Selected card {cardId} for slot {slotIndex}");
    }
    
    void SaveDeck()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerData();
        }
    }
    
    void RefreshAllSlots()
    {
        Debug.Log("Refreshing all deck slots");
        
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentPlayerData != null)
        {
            var deck = PlayerDataManager.Instance.currentPlayerData.cardDeck;
            
            if (deck != null) 
            {
                Debug.Log($"Current deck has {deck.Count} cards");
                for (int i = 0; i < deck.Count; i++)
                {
                    Debug.Log($"Deck slot {i}: {deck[i]}");
                }
            }
            else
            {
                Debug.LogWarning("Current deck is null");
                // Khởi tạo deck rỗng nếu null
                PlayerDataManager.Instance.currentPlayerData.cardDeck = new List<string>();
            }
        }
        
        foreach (DeckSlotUI slot in deckSlots)
        {
            slot.RefreshDisplay();
        }
    }
    
    void RefreshCardSelector()
    {
        foreach (CardSelectorUI item in cardSelectorItems)
        {
            item.UpdateSelectedState();
        }
    }
    
    void OnCardChanged(string cardId)
    {
        RefreshAllSlots();
        
        if (cardSelectorPanel != null && cardSelectorPanel.activeSelf)
        {
            LoadOwnedCards();
        }
    }
    
    public List<string> GetCurrentDeck()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentPlayerData != null)
        {
            var deck = PlayerDataManager.Instance.currentPlayerData.cardDeck;
            
            if (deck == null)
            {
                deck = new List<string>();
                PlayerDataManager.Instance.currentPlayerData.cardDeck = deck;
            }
            
            return deck;
        }
        
        return new List<string>();
    }
    
    void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnCardChanged -= OnCardChanged;
            PlayerDataManager.Instance.OnDeckChanged -= OnDeckChanged;
            PlayerDataManager.Instance.OnPlayerDataLoaded -= OnPlayerDataLoaded;
        }
    }
}