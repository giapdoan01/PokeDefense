using Firebase.Auth;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Register")]
    public GameObject registerPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public Button goToLoginButton;

    [Header("Login")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public Button goToRegisterButton;

    [Header("Status (Optional)")]
    public TextMeshProUGUI statusText;

    [Header("LoadingPanel")]
    public GameObject loadingPanel;
    private FirebaseAuth auth;
    private DatabaseReference databaseRef;

    void Start()
    {
        
        // Đăng ký các sự kiện nút ngay từ đầu
        registerButton.onClick.AddListener(RegisterAccount);
        goToLoginButton.onClick.AddListener(GoToLoginPanel);
        loginButton.onClick.AddListener(LoginAccount);
        goToRegisterButton.onClick.AddListener(GoToRegisterPanel);

        // Thiết lập UI ban đầu
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        loadingPanel.SetActive(false);
        
        // Kiểm tra trạng thái Firebase định kỳ
        InvokeRepeating(nameof(CheckFirebaseReady), 0.5f, 0.5f);
    }
    
    void CheckFirebaseReady()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsReady())
        {
            // Dừng việc kiểm tra định kỳ
            CancelInvoke(nameof(CheckFirebaseReady));
            
            // Khởi tạo Firebase Auth sau khi FirebaseManager đã sẵn sàng
            auth = FirebaseAuth.DefaultInstance;
            databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
    }


    public void RegisterAccount()
    {
        if (auth == null)
        {
            ShowStatus("Firebase chưa sẵn sàng. Vui lòng đợi...");
            return;
        }
        
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowStatus("Vui lòng điền đầy đủ!");
            return;
        }

        if (password != confirmPassword)
        {
            ShowStatus("Mật khẩu không khớp!");
            return;
        }

        if (password.Length < 6)
        {
            ShowStatus("Mật khẩu phải có ít nhất 6 ký tự!");
            return;
        }

        ShowStatus("Đang tạo tài khoản...");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowStatus("Đăng ký bị hủy!");
                return;
            }

            if (task.IsFaulted)
            {
                ShowStatus($"Lỗi: {GetErrorMessage(task.Exception)}");
                return;
            }

            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User;

                CreateNewPlayerData(newUser.UserId, email);
            }
        });
    }

    void CreateNewPlayerData(string userId, string email)
    {
        ShowStatus("Đang tạo dữ liệu...");

        PlayerData newPlayerData = new PlayerData
        {
            userId = userId,
            username = GetUsernameFromEmail(email),
            gem = 1000,
            unlockedMaps = new Dictionary<int, bool>(),
            ownedCardIds = new List<string>(),
            cardDeck = new System.Collections.Generic.List<string>()
        };

        string json = JsonUtility.ToJson(newPlayerData);

        databaseRef.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                ShowStatus($"Lỗi lưu dữ liệu!");
                return;
            }

            if (task.IsCompleted)
            {
                ShowStatus("Đăng ký thành công!");

                Invoke(nameof(GoToLoginPanel), 1.5f);
            }
        });
    }

    public void LoginAccount()
    {
        if (auth == null)
        {
            return;
        }
        
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Vui lòng điền đầy đủ!");
            return;
        }

        ShowStatus("Đang đăng nhập...");

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowStatus("Đăng nhập bị hủy!");
                return;
            }

            if (task.IsFaulted)
            {
                ShowStatus($"Lỗi: {GetErrorMessage(task.Exception)}");
                return;
            }

            if (task.IsCompleted)
            {
                FirebaseUser user = task.Result.User;

                LoadPlayerData(user.UserId);
            }
        });
    }

    void LoadPlayerData(string userId)
    {
        ShowStatus("Đang tải dữ liệu...");

        databaseRef.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                ShowStatus($"Lỗi tải dữ liệu!");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    PlayerData playerData = ParsePlayerData(snapshot);

                    // Lưu vào PlayerPrefs
                    string json = JsonUtility.ToJson(playerData);
                    PlayerPrefs.SetString("PlayerData", json);
                    PlayerPrefs.SetString("UserId", playerData.userId);
                    PlayerPrefs.Save();

                    ShowStatus("Đăng nhập thành công!");
                    loadingPanel.SetActive(true);
                    Invoke(nameof(LoadHomePage), 1f);
                }
                else
                {
                    ShowStatus("Không tìm thấy dữ liệu!");
                }
            }
        });
    }

    PlayerData ParsePlayerData(DataSnapshot snapshot)
    {
        PlayerData data = new PlayerData
        {
            userId = snapshot.Child("userId").Value?.ToString() ?? "",
            username = snapshot.Child("username").Value?.ToString() ?? "Player",
            gem = int.Parse(snapshot.Child("gem").Value?.ToString() ?? "0"),
            unlockedMapsList = new List<MapUnlockInfo>(),
            completedMapsList = new List<MapCompletionInfo>(),
            ownedCardIds = new List<string>(),
            cardDeck = new System.Collections.Generic.List<string>()
        };

        // Đọc unlockedMaps từ Firebase
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

        data.UpdateUnlockedMapsDict();
        data.UpdateCompletedMapsDict();

        // Đảm bảo Map 1 luôn được mở khóa
        data.unlockedMaps[1] = true;

        // Đọc ownedCardIds
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
        }

        // Đọc cardDeck
        if (snapshot.Child("cardDeck").Exists)
        {
            foreach (DataSnapshot deckSnapshot in snapshot.Child("cardDeck").Children)
            {
                string cardId = deckSnapshot.Value?.ToString();
                data.cardDeck.Add(cardId); // Kể cả null cũng thêm để giữ đúng index
            }
        }

        return data;
    }

    public void GoToRegisterPanel()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
        ClearInputs();
    }

    public void GoToLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        ClearInputs();
    }

    void LoadHomePage()
    {
        GameSceneManager.Instance.GotoHomePage();

    }

    void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void ClearInputs()
    {
        emailInput.text = "";
        passwordInput.text = "";
        confirmPasswordInput.text = "";
        loginEmailInput.text = "";
        loginPasswordInput.text = "";

        if (statusText != null)
            statusText.text = "";
    }

    string GetUsernameFromEmail(string email)
    {
        int atIndex = email.IndexOf('@');
        if (atIndex > 0)
        {
            return email.Substring(0, atIndex);
        }
        return "Player";
    }

    string GetErrorMessage(System.AggregateException exception)
    {
        if (exception.InnerException is FirebaseException firebaseEx)
        {
            var errorCode = (AuthError)firebaseEx.ErrorCode;

            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse:
                    return "Email đã được sử dụng!";
                case AuthError.InvalidEmail:
                    return "Email không hợp lệ!";
                case AuthError.WeakPassword:
                    return "Mật khẩu quá yếu!";
                case AuthError.WrongPassword:
                    return "Sai mật khẩu!";
                case AuthError.UserNotFound:
                    return "Không tìm thấy tài khoản!";
                default:
                    return errorCode.ToString();
            }
        }

        return exception.Message;
    }
}