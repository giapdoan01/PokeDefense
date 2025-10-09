using UnityEngine;
using System.Collections.Generic;

public class BlastoiseSkill : MonoBehaviour, ISkill
{
    [SerializeField] private float duration = 2f;
    [SerializeField] private float aoeRadius = 3f;
    
    private float damage;
    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    public void Initialize(float dmg, float rng, EnemyHealth target, Animator animator = null)
    {
        damage = dmg;
        
        if (target != null)
        {
            transform.position = target.transform.position + Vector3.up * 0.3f;
        }
        
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);
                enemy.TakeDamage(damage);
            }
        }
    }
}
