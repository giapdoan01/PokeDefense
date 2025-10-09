using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    
    [Header("Card Database - LOCAL")]
    public List<CardData> allCards = new List<CardData>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log($"CardManager initialized with {allCards.Count} cards");
    }
    
    public List<CardData> GetAllCards()
    {
        return new List<CardData>(allCards); // Return copy để tránh modify trực tiếp
    }

    public CardData GetCardById(string cardId)
    {
        return allCards.FirstOrDefault(card => card.id == cardId);
    }
    
    public List<CardData> GetCardsByType(string type)
    {
        return allCards.Where(card => 
            card.type.Equals(type, System.StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }
    
    public List<CardData> GetCardsByPriceRange(int minPrice, int maxPrice)
    {
        return allCards.Where(card => 
            card.gemPrice >= minPrice && card.gemPrice <= maxPrice
        ).ToList();
    }
    
    public List<CardData> SearchCardsByName(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
            return GetAllCards();
            
        return allCards.Where(card => 
            card.name.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0
        ).ToList();
    }
    

    public bool AddCard(CardData newCard)
    {
        if (allCards.Any(card => card.id == newCard.id))
        {
            Debug.LogWarning($"⚠️ Card ID {newCard.id} đã tồn tại!");
            return false;
        }
        
        allCards.Add(newCard);
        Debug.Log($" Added card: {newCard.name} (ID: {newCard.id})");
        return true;
    }
    
    public bool RemoveCard(string cardId)
    {
        CardData card = GetCardById(cardId);
        if (card != null)
        {
            allCards.Remove(card);
            Debug.Log($"Removed card: {card.name} (ID: {cardId})");
            return true;
        }
        
        Debug.LogWarning($"Card ID {cardId} không tồn tại!");
        return false;
    }
    
    public void ClearAllCards()
    {
        int count = allCards.Count;
        allCards.Clear();
        Debug.Log($"Cleared {count} cards");
    }
    
    public bool UpdateCard(string cardId, CardData updatedCard)
    {
        CardData existingCard = GetCardById(cardId);
        if (existingCard != null)
        {
            // Update fields
            existingCard.name = updatedCard.name;
            existingCard.type = updatedCard.type;
            existingCard.gemPrice = updatedCard.gemPrice;
            existingCard.cardImage = updatedCard.cardImage;
            
            Debug.Log($"Updated card: {cardId}");
            return true;
        }
        
        Debug.LogWarning($"Card ID {cardId} không tồn tại!");
        return false;
    }
    
    public int GetTotalCardCount()
    {
        return allCards.Count;
    }
    
    public bool CardExists(string cardId)
    {
        return allCards.Any(card => card.id == cardId);
    }
    
    public List<string> GetAllTypes()
    {
        return allCards.Select(card => card.type).Distinct().ToList();
    }
    
    public List<CardData> GetCardsSortedByPrice(bool ascending = true)
    {
        if (ascending)
            return allCards.OrderBy(card => card.gemPrice).ToList();
        else
            return allCards.OrderByDescending(card => card.gemPrice).ToList();
    }
}
