using UnityEngine;

public class CharizardSkill : MonoBehaviour, ISkill
{
    [Header("Settings")]
    [SerializeField] private float duration = 1f;
    
    private float damage;
    private float range;
    private EnemyHealth targetEnemy;
    private bool hasDealDamage = false;

    public void Initialize(float damage, float range, EnemyHealth target = null, Animator pokemonAnimator = null)
    {
        this.damage = damage;
        this.range = range;
        this.targetEnemy = target;
        
        Debug.Log($"🔥 CharizardSkill spawned at {transform.position} for target {(target != null ? target.name : "NULL")}");
        
        // Nếu có target, gây damage ngay
        if (targetEnemy != null)
        {
            DealDamage();
        }
        
        // Tự hủy sau duration
        Destroy(gameObject, duration);
    }

    private void DealDamage()
    {
        if (hasDealDamage) return;
        if (targetEnemy == null) return;
        if (!targetEnemy.gameObject.activeInHierarchy) return;

        targetEnemy.TakeDamage(damage);
        hasDealDamage = true;

        Debug.Log($"🔥 Charizard Skill dealt {damage} damage to {targetEnemy.name}");
    }
}
