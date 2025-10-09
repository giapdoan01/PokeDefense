using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;
    public TMP_Text totalCardsText;
    
    void Start()
    {   
        // Load cards
        ShowAllCards();
        
    }
    
    public void ShowAllCards()
    {
        List<CardData> cards = CardManager.Instance.GetAllCards();
        DisplayCards(cards);
    }
    
    public void ShowCardsByType(string type)
    {
        List<CardData> cards = CardManager.Instance.GetCardsByType(type);
        DisplayCards(cards);
    }
    void DisplayCards(List<CardData> cards)
    {
        StartCoroutine(DisplayCardsCoroutine(cards));
    }
    
    IEnumerator DisplayCardsCoroutine(List<CardData> cards)
    {
        // Clear old items
        foreach (Transform child in shopContainer)
        {
            Destroy(child.gameObject);
        }
        
        yield return null;
        
        // Check empty
        if (cards.Count == 0)
        {
            if (totalCardsText != null)
                totalCardsText.text = "Không có card nào";
            yield break;
        }
        
        // Update count
        if (totalCardsText != null)
            totalCardsText.text = $"Tổng: {cards.Count} cards";
        
        // Instantiate items
        foreach (CardData card in cards)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, shopContainer);
            
            // Force reset transform
            itemObj.transform.localScale = Vector3.one;
            itemObj.transform.localRotation = Quaternion.identity;
            
            // Setup UI
            ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(card);
            }
            
            yield return null;
        }
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(shopContainer.GetComponent<RectTransform>());
        
    }
}
