using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    
    private float currentHealth;
    private int roundMultiplier = 1;
    private bool isDead = false;

    [SerializeField] private Slider healthBar;

    public void SetStats(EnemyStats newStats)
    {
        stats = newStats;
        UpdateHealthFromStats();

        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.UpdateFromStats(stats);
        }
    }

    private void OnEnable()
    {
        isDead = false;
        UpdateHealthFromStats();
    }

    private void UpdateHealthFromStats()
    {
        if (stats != null)
        {
            currentHealth = Mathf.RoundToInt(stats.enemyHealth * roundMultiplier);
        }
        else
        {
            currentHealth = 100 * roundMultiplier;
        }
        
        if (healthBar != null)
        {
            healthBar.maxValue = currentHealth;
            healthBar.value = currentHealth;
        }
    }

    public void ApplyRoundMultiplier(int multiplier)
    {
        roundMultiplier = multiplier;
        UpdateHealthFromStats();
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        
        currentHealth -= dmg;
        if (healthBar != null) healthBar.value = currentHealth;

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }

    private void Die()
    {
        int moneyReward = (stats != null) ? stats.enemyMoney : 10;
        
        PlayerStats.Instance.AddCoin(moneyReward);

        gameObject.SetActive(false);
    }
    
    public EnemyStats Stats => stats;
}