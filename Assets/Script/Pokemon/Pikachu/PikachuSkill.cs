using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PikachuSkill : MonoBehaviour, ISkill
{
    [Header("Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float tickInterval = 0.5f;

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
    }

    public void DeactivateSkill()
    {
        isActive = false;
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
        var wait = new WaitForSeconds(tickInterval);

        while (isActive)
        {
            yield return wait;

            // Xóa enemies đã chết
            enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

            // Gây damage cho TẤT CẢ enemies trong vùng AOE
            foreach (var enemy in enemiesInRange)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
