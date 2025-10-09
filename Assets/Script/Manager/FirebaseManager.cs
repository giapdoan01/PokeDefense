using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

/// <summary>
/// Quản lý kết nối và thao tác với Firebase
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    
    private DatabaseReference databaseRef;
    private bool isFirebaseReady = false;
    
    void Awake()
    {
        // Singleton pattern
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
        InitializeFirebase();
    }
    
    /// <summary>
    /// Khởi tạo Firebase
    /// </summary>
    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Firebase sẵn sàng
                databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
                isFirebaseReady = true;
                Debug.Log("✅ Firebase đã sẵn sàng!");
            }
            else
            {
                Debug.LogError($"❌ Firebase lỗi: {task.Result}");
            }
        });
    }
    
    /// <summary>
    /// Kiểm tra Firebase đã sẵn sàng chưa
    /// </summary>
    public bool IsReady()
    {
        return isFirebaseReady;
    }
    
    /// <summary>
    /// Lấy reference đến node cards
    /// </summary>
    public DatabaseReference GetCardsReference()
    {
        return databaseRef.Child("cards");
    }
}
