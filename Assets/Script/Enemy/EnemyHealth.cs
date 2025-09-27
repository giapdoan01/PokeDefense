using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float baseHealth = 100;
    private float currentHealth;
    private int roundMultiplier = 1;

    [SerializeField] private Slider healthBar;

    private void OnEnable()
    {
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
        currentHealth -= dmg;
        if (healthBar != null) healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Cộng tiền cho player
        PlayerStats.Instance.AddMoney(10);

        // Trả về pool
        gameObject.SetActive(false);
    }
}
