using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string userId = "";
    public string username = "Player";
    public int gem = 0;
    
    public Dictionary<int, MapProgressData> mapProgress = new Dictionary<int, MapProgressData>();
    public List<string> ownedCardIds = new List<string>();
    public List<string> cardDeck = new List<string>();
    
    // MAP
    public bool IsMapUnlocked(int mapIndex)
    {
        return mapProgress.ContainsKey(mapIndex) && mapProgress[mapIndex].unlocked;
    }
    
    public void UnlockMap(int mapIndex)
    {
        if (!mapProgress.ContainsKey(mapIndex))
        {
            mapProgress[mapIndex] = new MapProgressData { mapIndex = mapIndex, unlocked = true };
        }
        else
        {
            mapProgress[mapIndex].unlocked = true;
        }
    }

    public int GetMapStars(int mapIndex)
    {
        return mapProgress.ContainsKey(mapIndex) ? mapProgress[mapIndex].stars : 0;
    }
    
    // CARD
    public bool HasCard(string cardId)
    {
        return ownedCardIds.Contains(cardId);
    }
    
    // Helper method: Thêm thẻ
    public void AddCard(string cardId)
    {
        if (!ownedCardIds.Contains(cardId))
        {
            ownedCardIds.Add(cardId);
        }
    }
    
    // Helper method: Xóa thẻ (nếu cần)
    public void RemoveCard(string cardId)
    {
        ownedCardIds.Remove(cardId);
    }
    // GEM
    public void AddGem(int amount)
    {
        gem += amount;
    }
    
    public bool SpendGem(int amount)
    {
        if (gem >= amount)
        {
            gem -= amount;
            return true;
        }
        return false;
    }
}

[Serializable]
public class MapProgressData
{
    public int mapIndex;
    public bool unlocked;
    public bool completed;
    public int stars;
}

[Serializable]
public class CardProgressData
{
    public string cardId;
    public bool owned;
    public int level;
}
