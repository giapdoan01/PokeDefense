using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardBagItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardTypeText;
    
    public CardData CardData { get; private set; }
    
    public void Setup(CardData cardData)
    {
        CardData = cardData;
        
        // Gán sprite
        if (cardImage != null && cardData.cardImage != null)
        {
            cardImage.sprite = cardData.cardImage;
        }
        
        // Gán tên
        if (cardNameText != null)
        {
            cardNameText.text = cardData.name;
        }
        
        // Gán type
        if (cardTypeText != null)
        {
            cardTypeText.text = cardData.type;
        }
    }
}
