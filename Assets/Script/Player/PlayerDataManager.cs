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
    public event Action<int, int> OnMapUnlocked;
    public event Action<string> OnCardChanged;
    public event Action OnDeckChanged; // Thêm event mới để thông báo khi deck thay đổi

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
            
            // Kiểm tra và khởi tạo các danh sách nếu cần
            if (currentPlayerData.ownedCardIds == null)
                currentPlayerData.ownedCardIds = new List<string>();
                
            if (currentPlayerData.cardDeck == null)
                currentPlayerData.cardDeck = new List<string>();
                
            // Log thông tin deck để debug
            Debug.Log($"PlayerData loaded: {currentPlayerData.username}");
            Debug.Log($"cardDeck count: {currentPlayerData.cardDeck.Count}");
            for (int i = 0; i < currentPlayerData.cardDeck.Count; i++)
            {
                Debug.Log($"Deck card {i}: {currentPlayerData.cardDeck[i]}");
            }

            OnPlayerDataLoaded?.Invoke(currentPlayerData);
            OnDeckChanged?.Invoke(); // Kích hoạt event deck thay đổi sau khi load
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy PlayerData!");
        }
    }

    void EnableRealtimeSync(string userId)
    {
        databaseRef.Child("users").Child(userId).ValueChanged += OnDataChanged;
        Debug.Log($"Realtime sync enabled");
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

            Debug.Log($"Realtime update");
            OnPlayerDataLoaded?.Invoke(currentPlayerData);
            OnDeckChanged?.Invoke(); // Kích hoạt event deck thay đổi sau khi update từ Firebase
        }
    }

    void SaveData()
    {
        if (currentPlayerData == null) return;

        // Đảm bảo cardDeck không null trước khi lưu
        if (currentPlayerData.cardDeck == null)
            currentPlayerData.cardDeck = new List<string>();
            
        // Debug trước khi lưu
        if (currentPlayerData.ownedCardIds != null)
            Debug.Log($"Saving player data with {currentPlayerData.ownedCardIds.Count} cards");
        else
            Debug.LogError("ownedCardIds is null when trying to save!");
            
        // Debug thông tin deck
        Debug.Log($"Saving deck with {currentPlayerData.cardDeck.Count} cards");
        for (int i = 0; i < currentPlayerData.cardDeck.Count; i++)
        {
            Debug.Log($"Saving deck card {i}: {currentPlayerData.cardDeck[i]}");
        }

        string json = JsonUtility.ToJson(currentPlayerData);

        // Debug json để kiểm tra
        Debug.Log($"JSON to save: {json}");

        databaseRef.Child("users").Child(currentPlayerData.userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Saved data");
                OnDeckChanged?.Invoke(); // Kích hoạt event sau khi lưu
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
        OnMapUnlocked?.Invoke(mapIndex, 0);
        SaveData();
    }

    public void CompleteMap(int mapIndex, int stars)
    {
        if (!currentPlayerData.mapProgress.ContainsKey(mapIndex))
        {
            currentPlayerData.mapProgress[mapIndex] = new MapProgressData { mapIndex = mapIndex };
        }

        currentPlayerData.mapProgress[mapIndex].completed = true;
        currentPlayerData.mapProgress[mapIndex].stars = Mathf.Max(currentPlayerData.mapProgress[mapIndex].stars, stars);

        OnMapUnlocked?.Invoke(mapIndex, stars);
        SaveData();
    }

    // ==================== CARD ====================

    public void AddCard(string cardId)
    {
        Debug.Log($"Trying to add card: {cardId}");
        Debug.Log($"Current owned cards: {(currentPlayerData.ownedCardIds != null ? currentPlayerData.ownedCardIds.Count : 0)}");

        // Kiểm tra xem danh sách có null không
        if (currentPlayerData.ownedCardIds == null)
            currentPlayerData.ownedCardIds = new List<string>();

        // Thêm thẻ
        currentPlayerData.AddCard(cardId);

        Debug.Log($"After adding, owned cards count: {currentPlayerData.ownedCardIds.Count}");
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
            mapProgress = new Dictionary<int, MapProgressData>(),
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

        // Đọc mapProgress từ Firebase
        if (snapshot.Child("mapProgress").Exists)
        {
            foreach (DataSnapshot mapSnapshot in snapshot.Child("mapProgress").Children)
            {
                if (int.TryParse(mapSnapshot.Key, out int mapIndex))
                {
                    MapProgressData mapData = new MapProgressData();
                    mapData.mapIndex = mapIndex;
                    mapData.unlocked = mapSnapshot.Child("unlocked").Value != null &&
                                       (bool)mapSnapshot.Child("unlocked").Value;
                    mapData.completed = mapSnapshot.Child("completed").Value != null &&
                                        (bool)mapSnapshot.Child("completed").Value;
                    mapData.stars = mapSnapshot.Child("stars").Value != null ?
                                    int.Parse(mapSnapshot.Child("stars").Value.ToString()) : 0;

                    data.mapProgress[mapIndex] = mapData;
                }
            }
        }

        return data;
    }

    void OnDestroy()
    {
        if (currentPlayerData != null)
        {
            databaseRef.Child("users").Child(currentPlayerData.userId).ValueChanged -= OnDataChanged;
        }
    }
}