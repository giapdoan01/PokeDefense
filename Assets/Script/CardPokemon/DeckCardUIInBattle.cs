using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckCardUIInBattle : MonoBehaviour
{
    [Header("UI Components")]
    public Image cardImage;
    public TMP_Text coinInGameText;
    public Image grayOverlay; // Ảnh phủ xám khi không đủ tiền
    
    [Header("Card Data")]
    private CardData cardData;
    private DragDropPokemon dragDropComponent;
    
    private void Start()
    {
        // Đảm bảo grayOverlay tồn tại
        if (grayOverlay == null)
        {
            Debug.LogWarning("Chưa gán grayOverlay, thẻ sẽ không hiển thị được trạng thái không đủ tiền");
        }
    }
    
    private void Update()
    {
        // Kiểm tra và cập nhật trạng thái tiền trong suốt game
        UpdateCardAvailability();
    }
    
    // Cập nhật trạng thái có thể dùng của thẻ dựa vào tiền hiện có
    private void UpdateCardAvailability()
    {
        if (cardData == null || PlayerStats.Instance == null) return;
        
        bool hasEnoughCoins = PlayerStats.Instance.coin >= cardData.coinInGame;
        
        // Hiển thị/ẩn lớp phủ xám
        if (grayOverlay != null)
        {
            grayOverlay.gameObject.SetActive(!hasEnoughCoins);
        }
    }
    
    public void Setup(CardData card)
    {
        if (card == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }
        
        cardData = card;
        
        // Hiển thị sprite
        if (cardImage != null && card.cardImage != null)
        {
            cardImage.sprite = card.cardImage;
        }
        else
        {
            Debug.LogWarning($"CardImage or CardSprite is null for card: {card.name}");
        }
        
        // Hiển thị chi phí coin (nếu có)
        if (coinInGameText != null)
        {
            coinInGameText.text = card.coinInGame.ToString();
        }
        
        // Lấy hoặc thêm DragDropPokemon component
        dragDropComponent = gameObject.GetComponent<DragDropPokemon>();
        if (dragDropComponent == null)
        {
            dragDropComponent = gameObject.AddComponent<DragDropPokemon>();
        }
        
        // Gán pokemonPrefab và ghostPrefab từ CardData
        if (card.pokemonPrefab != null)
        {
            dragDropComponent.pokemonPrefab = card.pokemonPrefab;
        }
        else
        {
            Debug.LogWarning($"pokemonPrefab is null for card: {card.name}");
        }
        
        if (card.pokemonGhostPrefab != null)
        {
            dragDropComponent.ghostPrefab = card.pokemonGhostPrefab;
        }
        else
        {
            Debug.LogWarning($"pokemonGhostPrefab is null for card: {card.name}");
        }
        
        // QUAN TRỌNG: Gán CardData cho DragDropPokemon
        dragDropComponent.SetCardData(card);
        
        // Kiểm tra ngay lập tức xem có đủ tiền không
        UpdateCardAvailability();
        
        Debug.Log($"Setup DeckCardUI for: {card.name}");
    }
    
    public CardData GetCardData()
    {
        return cardData;
    }
}