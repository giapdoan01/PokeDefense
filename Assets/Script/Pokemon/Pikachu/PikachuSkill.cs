using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PikachuSkill : MonoBehaviour, ISkill
{
    private float damage;
    private float range;
    private Transform targetEnemy;

    private float duration = 10f;       // Skill tồn tại 10 giây
    private float tickInterval = 0.5f;  // Gây damage mỗi 0.5 giây

    private List<EnemyHealth> enemiesInRange = new List<EnemyHealth>();

    public void Initialize(float dmg, float rng, EnemyHealth target, Animator animator = null)
    {
        damage = dmg;
        range = rng;
        targetEnemy = target.transform;

        // Spawn skill tại vị trí enemy target
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.position + Vector3.up * 0.3f; // nâng lên một chút cho dễ nhìn
        }

        StartCoroutine(DamageOverTime());
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null && !enemiesInRange.Contains(eh))
            {
                enemiesInRange.Add(eh);
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