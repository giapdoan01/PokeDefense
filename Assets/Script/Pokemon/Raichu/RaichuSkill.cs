using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaichuSkill : MonoBehaviour, ISkill
{
    private float damage;  // Gây 50 damage cố định
    private float range;
    private Transform targetEnemy;
    
    private float duration = 1.2f;  // Skill tồn tại 1.2 giây

    // Danh sách kẻ địch đã bị đánh để tránh đánh trúng nhiều lần
    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    public void Initialize(float dmg, float rng, EnemyHealth target, Animator animator = null)
    {
        // Vẫn sử dụng damage từ tham số nếu cần thay đổi
        if (dmg > 0) damage = dmg;
        range = rng;
        targetEnemy = target.transform;

        // Spawn skill tại vị trí enemy target
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.position + Vector3.up * 0.3f;
        }

        // Tự hủy sau một khoảng thời gian
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null && !hitEnemies.Contains(eh))
            {
                // Thêm vào danh sách để tránh gây damage nhiều lần
                hitEnemies.Add(eh);
                
                // Gây damage ngay lập tức
                eh.TakeDamage(damage);
                
                // Hiệu ứng trực quan khi đánh trúng (có thể thêm particle effect)
                Debug.Log($"Raichu's skill hit {other.name} with {damage} damage!");
            }
        }
    }
}