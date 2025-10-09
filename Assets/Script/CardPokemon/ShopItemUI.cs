using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image cardImage;
    public TMP_Text idText;
    public TMP_Text nameText;
    public TMP_Text typeText;
    public TMP_Text priceText;
    public Image backgroundImage;
    public Button buyButton;
    
    [Header("Type Colors")]
    public Color electricColor = new Color(1f, 0.9f, 0.2f);
    public Color fireColor = new Color(1f, 0.3f, 0.2f);
    public Color waterColor = new Color(0.2f, 0.6f, 1f);
    public Color grassColor = new Color(0.3f, 0.9f, 0.3f);
    public Color normalColor = new Color(0.7f, 0.7f, 0.7f);
    
    private CardData cardData;
    
    public void Setup(CardData card)
    {
        cardData = card;
        
        if (cardImage != null)
            cardImage.sprite = card.cardImage;
        
        if (idText != null)
            idText.text = $"#{card.id}";
        
        if (nameText != null)
            nameText.text = card.name;
        
        if (typeText != null)
            typeText.text = card.type;
        
        if (priceText != null)
            priceText.text = $"{card.gemPrice}";
        
        SetTypeColor(card.type);
        
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }
    
    void SetTypeColor(string type)
    {
        Color color = normalColor;
        
        switch (type.ToLower())
        {
            case "electric":
                color = electricColor;
                break;
            case "fire":
                color = fireColor;
                break;
            case "water":
                color = waterColor;
                break;
            case "grass":
                color = grassColor;
                break;
        }
        
        if (backgroundImage != null)
            backgroundImage.color = color;
    }
    
    void OnBuyClicked()
    {
        Debug.Log($"Mua card: {cardData.name} - Giá: {cardData.gemPrice} gems");
    }
}
