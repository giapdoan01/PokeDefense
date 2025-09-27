using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PikachuSkill : MonoBehaviour, ISkill
{
    private float damage;
    private float duration = 10f;       // Skill tồn tại 10 giây
    private float tickInterval = 0.5f;  // Gây sát thương mỗi 0.5s

    private List<EnemyHealth> enemiesInRange = new List<EnemyHealth>();

    public void Initialize(float dmg)
    {
        damage = dmg;
        StartCoroutine(DamageOverTime());
        Destroy(gameObject, duration); // tự hủy sau 10 giây
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null && !enemiesInRange.Contains(enemyHealth))
            {
                enemiesInRange.Add(enemyHealth);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemiesInRange.Contains(enemyHealth))
            {
                enemiesInRange.Remove(enemyHealth);
            }
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);

            foreach (var enemy in enemiesInRange)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
