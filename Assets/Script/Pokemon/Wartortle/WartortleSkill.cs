using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WartortleSkill : MonoBehaviour, ISkill
{
    [Header("Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float slowPercent = 0.5f; // 50% tốc độ

    private float damage;
    private float range;
    private List<EnemyHealth> enemiesInRange = new List<EnemyHealth>();
    private bool isActive;

    public void Initialize(float damage, float range, EnemyHealth target = null, Animator pokemonAnimator = null)
    {
        this.damage = damage;
        this.range = range;
        this.isActive = true;

        // Spawn tại vị trí target (vị trí cố định, KHÔNG DI CHUYỂN)
        if (target != null)
        {
            transform.position = target.transform.position + Vector3.up * 0.3f;
        }

        StartCoroutine(DamageOverTime());
        Destroy(gameObject, duration);
        
        // ✅ Reset animation ngay sau khi spawn
        if (pokemonAnimator != null)
        {
            pokemonAnimator.SetBool("IsAttacking", false);
        }
    }

    public void DeactivateSkill()
    {
        isActive = false;
        
        // ✅ XÓA SLOW EFFECT KHI SKILL BỊ HỦY
        foreach (var enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                EnemyController ec = enemy.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.RemoveSlowEffect(this);
                }
            }
        }
        
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null && !enemiesInRange.Contains(eh))
            {
                enemiesInRange.Add(eh);
                
                // ✅ ÁP DỤNG SLOW EFFECT
                EnemyController ec = other.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.ApplySlowEffect(this, slowPercent);
                    Debug.Log($"Slowed {other.name} to {slowPercent * 100}% speed");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                enemiesInRange.Remove(eh);
                
                EnemyController ec = other.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.RemoveSlowEffect(this);
                    Debug.Log($"🐢 {other.name} speed restored");
                }
            }
        }
    }

    private IEnumerator DamageOverTime()
    {
        var wait = new WaitForSeconds(tickInterval);

        while (isActive)
        {
            yield return wait;

            List<EnemyHealth> deadEnemies = new List<EnemyHealth>();
            foreach (var enemy in enemiesInRange)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    deadEnemies.Add(enemy);
                }
            }
            foreach (var deadEnemy in deadEnemies)
            {
                if (deadEnemy != null)
                {
                    EnemyController ec = deadEnemy.GetComponent<EnemyController>();
                    if (ec != null)
                    {
                        ec.RemoveSlowEffect(this);
                    }
                }
                enemiesInRange.Remove(deadEnemy);
            }
            foreach (var enemy in enemiesInRange)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    
    private void OnDestroy()
    {
        foreach (var enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                EnemyController ec = enemy.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.RemoveSlowEffect(this);
                }
            }
        }
    }
}
