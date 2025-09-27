using UnityEngine;
using TMPro; // Import đúng cho TextMeshPro

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public TextMeshProUGUI moneyText; // Sử dụng TextMeshProUGUI cho UI

    public int money = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyDisplay(); // Cập nhật hiển thị tiền
        Debug.Log("Money: " + money);
    }

    // Phương thức cập nhật hiển thị tiền
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
    }

    // Thêm Start để cập nhật hiển thị ban đầu
    private void Start()
    {
        UpdateMoneyDisplay();
    }
}