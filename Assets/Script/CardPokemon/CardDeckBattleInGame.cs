using System.Collections.Generic;
using UnityEngine;

public class CardDeckBattleInGame : MonoBehaviour
{
    public static CardDeckBattleInGame Instance;
    
    [Header("Deck UI Container")]
    public Transform deckUIContainer;
    public GameObject deckCardUIPrefab;
    
    private List<DeckCardUIInBattle> deckCardUIs = new List<DeckCardUIInBattle>();
    private List<CardData> currentBattleDeck = new List<CardData>();
    
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
        LoadDeckFromPlayerData();
    }
    
    /// <summary>
    /// Load Deck từ PlayerData và tạo UI
    /// </summary>
    void LoadDeckFromPlayerData()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentPlayerData == null)
        {
            Debug.LogError("❌ PlayerDataManager not found!");
            return;
        }
        
        if (CardManager.Instance == null)
        {
            Debug.LogError("❌ CardManager not found!");
            return;
        }
        
        // Lấy cardDeck từ PlayerData
        List<string> cardDeck = PlayerDataManager.Instance.currentPlayerData.cardDeck;
        
        if (cardDeck == null || cardDeck.Count == 0)
        {
            Debug.LogWarning("⚠️ Deck is empty!");
            return;
        }
        
        // Clear old UI
        ClearDeckUI();
        
        // Tạo UI cho từng card trong deck
        foreach (string cardId in cardDeck)
        {
            if (string.IsNullOrEmpty(cardId)) continue;
            
            CardData card = CardManager.Instance.GetCardById(cardId);
            
            if (card != null)
            {
                currentBattleDeck.Add(card);
                CreateDeckCardUI(card);
            }
            else
            {
                Debug.LogWarning($"⚠️ Card not found: {cardId}");
            }
        }
        
        Debug.Log($"✅ Loaded {currentBattleDeck.Count} cards into battle deck");
    }
    
    /// <summary>
    /// Tạo UI cho 1 card trong deck
    /// </summary>
    void CreateDeckCardUI(CardData card)
    {
        if (deckCardUIPrefab == null || deckUIContainer == null)
        {
            Debug.LogError("❌ Deck UI Prefab or Container is null!");
            return;
        }
        
        GameObject cardObj = Instantiate(deckCardUIPrefab, deckUIContainer);
        DeckCardUIInBattle cardUI = cardObj.GetComponent<DeckCardUIInBattle>();
        
        if (cardUI != null)
        {
            cardUI.Setup(card);
            deckCardUIs.Add(cardUI);
        }
        else
        {
            Debug.LogError("❌ DeckCardUI component not found on prefab!");
        }
    }
    
    /// <summary>
    /// Xóa toàn bộ UI deck cũ
    /// </summary>
    void ClearDeckUI()
    {
        foreach (Transform child in deckUIContainer)
        {
            Destroy(child.gameObject);
        }
        
        deckCardUIs.Clear();
        currentBattleDeck.Clear();
    }
    
    /// <summary>
    /// Lấy danh sách CardData trong deck hiện tại
    /// </summary>
    public List<CardData> GetCurrentBattleDeck()
    {
        return currentBattleDeck;
    }
    
    /// <summary>
    /// Refresh deck (gọi khi có thay đổi)
    /// </summary>
    public void RefreshDeck()
    {
        LoadDeckFromPlayerData();
    }
}
