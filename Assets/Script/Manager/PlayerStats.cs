using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    
    [Header("Player Resources")]
    public int coin = 100; // Tiền ban đầu
    
    [Header("UI")]
    public TextMeshProUGUI coinText;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateCoinUI();
    }
    
    /// <summary>
    /// Trừ tiền khi mua card
    /// </summary>
    public bool SpendCoin(int amount)
    {
        if (coin >= amount)
        {
            coin -= amount;
            UpdateCoinUI();
            Debug.Log($"💰 Spent {amount} coins. Remaining: {coin}");
            return true;
        }
        else
        {
            Debug.LogWarning($"⚠️ Not enough coins! Need: {amount}, Have: {coin}");
            return false;
        }
    }
    
    /// <summary>
    /// Thêm tiền (VD: từ hệ thống thu hoạch)
    /// </summary>
    public void AddCoin(int amount)
    {
        coin += amount;
        UpdateCoinUI();
        Debug.Log($"💰 Gained {amount} coins. Total: {coin}");
    }
    
    /// <summary>
    /// Cập nhật UI hiển thị tiền
    /// </summary>
    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
    }
}
