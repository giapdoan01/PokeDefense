using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapUnlockInfo
{
    public int mapIndex;
    public bool isUnlocked;
}

[System.Serializable]
public class MapCompletionInfo
{
    public int mapIndex;
    public bool isCompleted;
}

[System.Serializable]
public class PlayerData
{
    public string userId;
    public string username;
    public int gem;
    public List<string> ownedCardIds = new List<string>();
    public List<string> cardDeck = new List<string>();
    
    // Sử dụng List thay vì Dictionary để hỗ trợ serialization
    public List<MapUnlockInfo> unlockedMapsList = new List<MapUnlockInfo>();
    public List<MapCompletionInfo> completedMapsList = new List<MapCompletionInfo>();
    
    // Dictionary sử dụng trong runtime, không được serialization
    [System.NonSerialized]
    public Dictionary<int, bool> unlockedMaps = new Dictionary<int, bool>();
    [System.NonSerialized]
    public Dictionary<int, bool> completedMaps = new Dictionary<int, bool>();
    
    // Các phương thức xử lý gem
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
    
    // Các phương thức xử lý map
    public void UnlockMap(int mapIndex)
    {
        if (unlockedMaps == null)
            unlockedMaps = new Dictionary<int, bool>();
            
        unlockedMaps[mapIndex] = true;
        
        // Cập nhật list cho serialization
        UpdateUnlockedMapsList();
    }
    
    public bool IsMapUnlocked(int mapIndex)
    {
        if (unlockedMaps == null)
        {
            unlockedMaps = new Dictionary<int, bool>();
            UpdateUnlockedMapsDict();
        }
            
        return mapIndex == 1 || (unlockedMaps.ContainsKey(mapIndex) && unlockedMaps[mapIndex]);
    }
    
    // THÊM MỚI: Các phương thức xử lý map đã hoàn thành
    public void MarkMapCompleted(int mapIndex)
    {
        if (completedMaps == null)
            completedMaps = new Dictionary<int, bool>();
            
        completedMaps[mapIndex] = true;
        
        // Cập nhật list cho serialization
        UpdateCompletedMapsList();
    }
    
    public bool IsMapCompleted(int mapIndex)
    {
        if (completedMaps == null)
        {
            completedMaps = new Dictionary<int, bool>();
            UpdateCompletedMapsDict();
        }
            
        return completedMaps.ContainsKey(mapIndex) && completedMaps[mapIndex];
    }
    
    // Chuyển đổi giữa Dictionary và List cho serialization
    public void UpdateUnlockedMapsList()
    {
        if (unlockedMapsList == null)
            unlockedMapsList = new List<MapUnlockInfo>();
            
        unlockedMapsList.Clear();
        
        foreach (var kvp in unlockedMaps)
        {
            unlockedMapsList.Add(new MapUnlockInfo { mapIndex = kvp.Key, isUnlocked = kvp.Value });
        }
    }
    
    public void UpdateUnlockedMapsDict()
    {
        if (unlockedMaps == null)
            unlockedMaps = new Dictionary<int, bool>();
            
        unlockedMaps.Clear();
        
        if (unlockedMapsList != null)
        {
            foreach (var info in unlockedMapsList)
            {
                unlockedMaps[info.mapIndex] = info.isUnlocked;
            }
        }
        
        // Đảm bảo Map 1 luôn được mở khóa
        unlockedMaps[1] = true;
    }
    
    // THÊM MỚI: Chuyển đổi giữa Dictionary và List cho completedMaps
    public void UpdateCompletedMapsList()
    {
        if (completedMapsList == null)
            completedMapsList = new List<MapCompletionInfo>();
            
        completedMapsList.Clear();
        
        foreach (var kvp in completedMaps)
        {
            completedMapsList.Add(new MapCompletionInfo { mapIndex = kvp.Key, isCompleted = kvp.Value });
        }
    }
    
    public void UpdateCompletedMapsDict()
    {
        if (completedMaps == null)
            completedMaps = new Dictionary<int, bool>();
            
        completedMaps.Clear();
        
        if (completedMapsList != null)
        {
            foreach (var info in completedMapsList)
            {
                completedMaps[info.mapIndex] = info.isCompleted;
            }
        }
    }
    
    // Các phương thức xử lý thẻ bài
    public void AddCard(string cardId)
    {
        if (ownedCardIds == null)
            ownedCardIds = new List<string>();
            
        if (!ownedCardIds.Contains(cardId))
            ownedCardIds.Add(cardId);
    }
    
    public bool HasCard(string cardId)
    {
        return ownedCardIds != null && ownedCardIds.Contains(cardId);
    }
}