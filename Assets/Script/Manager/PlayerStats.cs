using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    
    [Header("Player Resources")]
    public int coin = 0;
    
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

    public bool SpendCoin(int amount)
    {
        if (coin >= amount)
        {
            coin -= amount;
            UpdateCoinUI();
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough coins! Need: {amount}, Have: {coin}");
            return false;
        }
    }

    public void AddCoin(int amount)
    {
        coin += amount;
        UpdateCoinUI();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
    }
}
