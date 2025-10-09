using UnityEngine;
using System.Collections;

public class BulbasaurSkill : MonoBehaviour, ISkill
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 5f; // Tốc độ di chuyển 5 unit/s

    private float damage;
    private float range;
    private EnemyHealth target;
    private Animator pokemonAnimator;
    private bool hasHitTarget = false;

    public void Initialize(float baseDamage, float baseRange, EnemyHealth currentTarget, Animator animator)
    {
        damage = baseDamage;
        range = baseRange;
        target = currentTarget;
        pokemonAnimator = animator;
        
        // Thiết lập rotation ban đầu với X = -90
        ApplyFixedRotation();
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy || hasHitTarget)
        {
            // Nếu mục tiêu không tồn tại hoặc đã hit, tự hủy
            Destroy(gameObject);
            return;
        }

        // Di chuyển về phía mục tiêu
        Vector3 targetDirection = (target.transform.position - transform.position).normalized;
        transform.position += targetDirection * speed * Time.deltaTime;
        
        // Hướng projectile về phía mục tiêu nhưng giữ X = -90
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = targetRotation;
        ApplyFixedRotation();
        
        // Kiểm tra khoảng cách tới mục tiêu
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < 0.5f) // Điểm va chạm
        {
            HitTarget();
        }
    }

    private void ApplyFixedRotation()
    {
        // Áp dụng rotation X = -90 nhưng giữ nguyên rotation Y và Z
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(90, currentRotation.y, currentRotation.z);
    }

    private void HitTarget()
    {
        if (hasHitTarget) return;
        
        hasHitTarget = true;
        
        // Gây sát thương cho mục tiêu
        target.TakeDamage((int)damage);
        
        // Hủy projectile
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra va chạm với mục tiêu dựa trên collider
        if (!hasHitTarget && other.gameObject == target.gameObject)
        {
            HitTarget();
        }
    }

    // Vẽ Gizmo để debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}