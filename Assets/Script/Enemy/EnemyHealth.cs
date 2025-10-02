using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float baseHealth = 100;
    private float currentHealth;
    private int roundMultiplier = 1;
    private bool isDead = false; // Thêm biến cờ để kiểm tra đã chết chưa

    [SerializeField] private Slider healthBar;

    private void OnEnable()
    {
        isDead = false; // Reset biến cờ khi quái được tái kích hoạt từ pool
        currentHealth = Mathf.RoundToInt(baseHealth * roundMultiplier);
        if (healthBar != null)
        {
            healthBar.maxValue = currentHealth;
            healthBar.value = currentHealth;
        }
    }

    public void ApplyRoundMultiplier(int multiplier)
    {
        roundMultiplier = multiplier;
    }

    public void TakeDamage(float dmg)
    {
        // Nếu quái đã chết rồi, không xử lý damage nữa
        if (isDead) return;
        
        currentHealth -= dmg;
        if (healthBar != null) healthBar.value = currentHealth;

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true; // Đánh dấu đã chết trước khi gọi Die()
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Enemy {gameObject.name} died, adding 10 money");
        // Cộng tiền cho player
        PlayerStats.Instance.AddMoney(10);

        // Trả về pool
        gameObject.SetActive(false);
    }
}