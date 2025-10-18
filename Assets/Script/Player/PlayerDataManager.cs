using System;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Current Player Data")]
    public PlayerData currentPlayerData;

    private DatabaseReference databaseRef;

    public event Action<PlayerData> OnPlayerDataLoaded;
    public event Action<int> OnGemChanged;
    public event Action<int> OnMapUnlocked;
    public event Action<string> OnCardChanged;
    public event Action OnDeckChanged;

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
            return;
        }

        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Start()
    {
        LoadPlayerDataFromPrefs();

        if (currentPlayerData != null)
        {
            EnableRealtimeSync(currentPlayerData.userId);
        }
    }

    void LoadPlayerDataFromPrefs()
    {
        string json = PlayerPrefs.GetString("PlayerData", "");

        if (!string.IsNullOrEmpty(json))
        {
            currentPlayerData = JsonUtility.FromJson<PlayerData>(json);

            if (currentPlayerData.ownedCardIds == null)
                currentPlayerData.ownedCardIds = new List<string>();

            if (currentPlayerData.cardDeck == null)
                currentPlayerData.cardDeck = new List<string>();

            if (currentPlayerData.unlockedMapsList == null)
                currentPlayerData.unlockedMapsList = new List<MapUnlockInfo>();

            if (currentPlayerData.completedMapsList == null)
                currentPlayerData.completedMapsList = new List<MapCompletionInfo>();

            // Cập nhật Dictionary từ List
            currentPlayerData.UpdateUnlockedMapsDict();
            currentPlayerData.UpdateCompletedMapsDict();

            OnPlayerDataLoaded?.Invoke(currentPlayerData);
            OnDeckChanged?.Invoke();
        }
    }

    void EnableRealtimeSync(string userId)
    {
        databaseRef.Child("users").Child(userId).ValueChanged += OnDataChanged;
    }

    void OnDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Realtime error: {args.DatabaseError.Message}");
            return;
        }

        if (args.Snapshot.Exists)
        {
            PlayerData newData = ParsePlayerData(args.Snapshot);
            currentPlayerData = newData;

            OnPlayerDataLoaded?.Invoke(currentPlayerData);
            OnDeckChanged?.Invoke();
        }
    }

    void SaveData()
    {
        if (currentPlayerData == null) return;

        if (currentPlayerData.cardDeck == null)
            currentPlayerData.cardDeck = new List<string>();

        currentPlayerData.UpdateUnlockedMapsList();
        currentPlayerData.UpdateCompletedMapsList();

        if (currentPlayerData.ownedCardIds != null)
            Debug.Log($"Saving player data with {currentPlayerData.ownedCardIds.Count} cards");
        else
            Debug.LogError("ownedCardIds is null when trying to save!");

        for (int i = 0; i < currentPlayerData.cardDeck.Count; i++)
        {
            Debug.Log($"Saving deck card {i}: {currentPlayerData.cardDeck[i]}");
        }

        string json = JsonUtility.ToJson(currentPlayerData);

        databaseRef.Child("users").Child(currentPlayerData.userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                OnDeckChanged?.Invoke();
            }
        });

        // Lưu cả vào PlayerPrefs
        PlayerPrefs.SetString("PlayerData", json);
        PlayerPrefs.Save();
    }

    // Công khai phương thức SaveData để DeckManager có thể gọi trực tiếp
    public void SavePlayerData()
    {
        SaveData();
    }

    // ==================== GEM ====================

    public void AddGem(int amount)
    {
        currentPlayerData.AddGem(amount);
        OnGemChanged?.Invoke(currentPlayerData.gem);
        SaveData();
    }

    public bool SpendGem(int amount)
    {
        bool success = currentPlayerData.SpendGem(amount);
        if (success)
        {
            OnGemChanged?.Invoke(currentPlayerData.gem);
            SaveData();
        }
        return success;
    }

    // ==================== MAP ====================

    public void UnlockMap(int mapIndex)
    {
        currentPlayerData.UnlockMap(mapIndex);
        OnMapUnlocked?.Invoke(mapIndex);
        SaveData();
    }

    // ==================== CARD ====================

    public void AddCard(string cardId)
    {
        // Kiểm tra xem danh sách có null không
        if (currentPlayerData.ownedCardIds == null)
            currentPlayerData.ownedCardIds = new List<string>();

        // Thêm thẻ
        currentPlayerData.AddCard(cardId);

        OnCardChanged?.Invoke(cardId);
        SaveData();
    }

    // ==================== DECK ====================

    public void UpdateDeck(List<string> newDeck)
    {
        if (currentPlayerData == null) return;

        currentPlayerData.cardDeck = newDeck;
        OnDeckChanged?.Invoke();
        SaveData();
    }

    // ==================== GETTERS ====================

    public int GetGem() => currentPlayerData?.gem ?? 0;
    public string GetUsername() => currentPlayerData?.username ?? "Player";
    public bool IsMapUnlocked(int mapIndex) => currentPlayerData?.IsMapUnlocked(mapIndex) ?? false;
    public bool HasCard(string cardId) => currentPlayerData?.HasCard(cardId) ?? false;

    // ==================== PARSE ====================

    PlayerData ParsePlayerData(DataSnapshot snapshot)
    {
        PlayerData data = new PlayerData
        {
            userId = snapshot.Child("userId").Value?.ToString() ?? "",
            username = snapshot.Child("username").Value?.ToString() ?? "Player",
            gem = int.Parse(snapshot.Child("gem").Value?.ToString() ?? "0"),
            unlockedMaps = new Dictionary<int, bool>(),
            completedMaps = new Dictionary<int, bool>(),
            ownedCardIds = new List<string>(),
            cardDeck = new List<string>()
        };

        // Đọc ownedCardIds từ Firebase
        if (snapshot.Child("ownedCardIds").Exists)
        {
            foreach (DataSnapshot cardSnapshot in snapshot.Child("ownedCardIds").Children)
            {
                string cardId = cardSnapshot.Value?.ToString();
                if (!string.IsNullOrEmpty(cardId))
                {
                    data.ownedCardIds.Add(cardId);
                }
            }
            Debug.Log($"Loaded {data.ownedCardIds.Count} owned cards from Firebase");
        }

        // Đọc cardDeck từ Firebase
        if (snapshot.Child("cardDeck").Exists)
        {
            foreach (DataSnapshot deckSnapshot in snapshot.Child("cardDeck").Children)
            {
                string cardId = deckSnapshot.Value?.ToString();
                data.cardDeck.Add(cardId); // Kể cả null cũng thêm để giữ đúng index
            }
            Debug.Log($"Loaded {data.cardDeck.Count} deck cards from Firebase");
            for (int i = 0; i < data.cardDeck.Count; i++)
            {
                Debug.Log($"Deck card {i}: {data.cardDeck[i]}");
            }
        }

        // Đọc unlockedMapsList từ Firebase
        if (snapshot.Child("unlockedMapsList").Exists)
        {
            foreach (DataSnapshot mapSnapshot in snapshot.Child("unlockedMapsList").Children)
            {
                int mapIndex = int.Parse(mapSnapshot.Child("mapIndex").Value?.ToString() ?? "0");
                bool isUnlocked = bool.Parse(mapSnapshot.Child("isUnlocked").Value?.ToString() ?? "false");

                MapUnlockInfo info = new MapUnlockInfo { mapIndex = mapIndex, isUnlocked = isUnlocked };
                data.unlockedMapsList.Add(info);
            }
        }
        data.UpdateUnlockedMapsDict();

        // THÊM MỚI: Đọc completedMapsList từ Firebase
        if (snapshot.Child("completedMapsList").Exists)
        {
            foreach (DataSnapshot mapSnapshot in snapshot.Child("completedMapsList").Children)
            {
                int mapIndex = int.Parse(mapSnapshot.Child("mapIndex").Value?.ToString() ?? "0");
                bool isCompleted = bool.Parse(mapSnapshot.Child("isCompleted").Value?.ToString() ?? "false");

                MapCompletionInfo info = new MapCompletionInfo { mapIndex = mapIndex, isCompleted = isCompleted };
                data.completedMapsList.Add(info);
            }
        }
        data.UpdateCompletedMapsDict();

        return data;
    }
    // THÊM MỚI: Phương thức để đánh dấu map đã hoàn thành
    public void MarkMapCompleted(int mapIndex)
    {
        currentPlayerData.MarkMapCompleted(mapIndex);
        SaveData();
    }

    // THÊM MỚI: Phương thức để kiểm tra map đã hoàn thành chưa
    public bool IsMapCompleted(int mapIndex)
    {
        return currentPlayerData?.IsMapCompleted(mapIndex) ?? false;
    }

    void OnDestroy()
    {
        if (currentPlayerData != null)
        {
            databaseRef.Child("users").Child(currentPlayerData.userId).ValueChanged -= OnDataChanged;
        }
    }
}