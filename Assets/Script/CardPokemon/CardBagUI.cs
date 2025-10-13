using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardBagUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform cardContainer; // Grid Layout Group
    [SerializeField] private GameObject cardItemPrefab;
    
    private List<CardBagItemUI> cardItems = new List<CardBagItemUI>();
    
    void Start()
    {
        PlayerDataManager.Instance.OnPlayerDataLoaded += LoadCardBag;
        
        if (PlayerDataManager.Instance.currentPlayerData != null)
        {
            LoadCardBag(PlayerDataManager.Instance.currentPlayerData);
        }
    }
    
    void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataLoaded -= LoadCardBag;
        }
    }
    
    void LoadCardBag(PlayerData playerData)
    {
        if (playerData == null || playerData.ownedCardIds == null)
        {
            Debug.LogWarning("PlayerData hoặc ownedCardIds null!");
            return;
        }
        
        ClearCardItems();
        
        Debug.Log($"Loading {playerData.ownedCardIds.Count} cards from bag");
        
        // Hiển thị tất cả card sở hữu
        foreach (string cardId in playerData.ownedCardIds)
        {
            CardData cardData = CardManager.Instance.GetCardById(cardId);
            
            if (cardData != null)
            {
                CreateCardItem(cardData);
            }
        }
        
        Debug.Log($"Loaded {cardItems.Count} cards to bag UI");
    }
    
    void CreateCardItem(CardData cardData)
    {
        GameObject itemObj = Instantiate(cardItemPrefab, cardContainer);
        CardBagItemUI cardItem = itemObj.GetComponent<CardBagItemUI>();
        
        if (cardItem != null)
        {
            cardItem.Setup(cardData);
            cardItems.Add(cardItem);
        }
    }
    
    void ClearCardItems()
    {
        foreach (var item in cardItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        cardItems.Clear();
    }
    
    public void RefreshBag()
    {
        if (PlayerDataManager.Instance.currentPlayerData != null)
        {
            LoadCardBag(PlayerDataManager.Instance.currentPlayerData);
        }
    }
}