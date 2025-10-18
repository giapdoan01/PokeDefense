using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardBagUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform cardContainer; // Grid Layout Group
    [SerializeField] private GameObject cardItemPrefab; // Prefab cho mỗi card item
    
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

    // Hàm tải và hiển thị card từ PlayerData
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

    // Tạo một item UI cho card và thêm vào container
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

    // Xóa tất cả item UI hiện có (Dành cho Admin hoặc refresh)
    void ClearCardItems()
    {
        foreach (var item in cardItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        cardItems.Clear();
    }
    
    // Hàm refresh lại UI (có thể gọi khi có thay đổi trong PlayerData)
    public void RefreshBag()
    {
        if (PlayerDataManager.Instance.currentPlayerData != null)
        {
            LoadCardBag(PlayerDataManager.Instance.currentPlayerData);
        }
    }
}