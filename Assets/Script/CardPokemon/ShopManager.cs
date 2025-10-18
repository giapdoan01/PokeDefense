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
    public Button ReturnHomeButton;
    public TMP_Text GemText;

    void Start()
    {
        ShowAllCards();
        if (ReturnHomeButton != null)
            ReturnHomeButton.onClick.AddListener(OnReturnHomeClicked);

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataLoaded += UpdateUI;
            PlayerDataManager.Instance.OnGemChanged += UpdateGem;

            if (PlayerDataManager.Instance.currentPlayerData != null)
            {
                UpdateUI(PlayerDataManager.Instance.currentPlayerData);
            }
        }
    }
    void UpdateUI(PlayerData data)
    {
        if (GemText != null)
            GemText.text = data.gem.ToString();
    }
    void UpdateGem(int gem)
    {
        if (GemText != null)
            GemText.text = gem.ToString();
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
        foreach (Transform child in shopContainer)
        {
            Destroy(child.gameObject);
        }

        yield return null;

        if (cards.Count == 0)
        {
            if (totalCardsText != null)
                totalCardsText.text = "Không có card nào";
            yield break;
        }

        if (totalCardsText != null)
            totalCardsText.text = $"Tổng: {cards.Count} cards";

        foreach (CardData card in cards)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, shopContainer);

            itemObj.transform.localScale = Vector3.one;
            itemObj.transform.localRotation = Quaternion.identity;

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
    void OnReturnHomeClicked()
    {
        GameSceneManager.Instance.GotoHomePage();
    }
}
