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

    // Load deck từ PlayerData và tạo UI
    void LoadDeckFromPlayerData()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentPlayerData == null)
        {
            Debug.LogError("PlayerDataManager not found!");
            return;
        }

        if (CardManager.Instance == null)
        {
            Debug.LogError("CardManager not found!");
            return;
        }

        // Lấy cardDeck từ PlayerData
        List<string> cardDeck = PlayerDataManager.Instance.currentPlayerData.cardDeck;

        if (cardDeck == null || cardDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
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
                Debug.LogWarning($"Card not found: {cardId}");
            }
        }

        Debug.Log($"Loaded {currentBattleDeck.Count} cards into battle deck");
    }
    // Tạo UI cho một card và thêm vào container
    void CreateDeckCardUI(CardData card)
    {
        if (deckCardUIPrefab == null || deckUIContainer == null)
        {
            Debug.LogError("Deck UI Prefab or Container is null!");
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
            Debug.LogError("DeckCardUI component not found on prefab!");
        }
    }
    // Xoá tất cả UI card trong deck
    void ClearDeckUI()
    {
        foreach (Transform child in deckUIContainer)
        {
            Destroy(child.gameObject);
        }
        
        deckCardUIs.Clear();
        currentBattleDeck.Clear();
    }
    
    public List<CardData> GetCurrentBattleDeck()
    {
        return currentBattleDeck;
    }
    
    public void RefreshDeck()
    {
        LoadDeckFromPlayerData();
    }
}
