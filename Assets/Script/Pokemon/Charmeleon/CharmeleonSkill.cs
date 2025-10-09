using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharmeleonSkill : MonoBehaviour, ISkill
{
    [Header("Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float heightOffset = 0.3f; // Offset trên đầu enemy
    [SerializeField] private float moveSpeed = 10f; // Tốc độ di chuyển khi đổi target

    private float damage;
    private float range;
    private List<EnemyHealth> enemiesInRange = new List<EnemyHealth>();
    private bool isActive;
    private EnemyHealth currentTarget; // Target hiện tại để theo dõi
    private Animator pokemonAnimator; // Lưu trữ tham chiếu đến animator

    public void Initialize(float damage, float range, EnemyHealth target = null, Animator pokemonAnimator = null)
    {
        this.damage = damage;
        this.range = range;
        this.isActive = true;
        this.currentTarget = target;
        this.pokemonAnimator = pokemonAnimator; // Lưu trữ animator để sử dụng sau này

        // Spawn tại vị trí target (khởi tạo vị trí ban đầu)
        if (target != null)
        {
            transform.position = target.transform.position + Vector3.up * heightOffset;
        }

        // Thiết lập rotation x = -90 độ khi sinh ra
        transform.rotation = Quaternion.Euler(-90, 0, 0);

        StartCoroutine(DamageOverTime());
        StartCoroutine(FollowTarget()); // Thêm coroutine theo dõi target
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

        // Đảm bảo animation trở về idle khi kết thúc
        if (pokemonAnimator != null)
        {
            pokemonAnimator.SetBool("IsAttacking", false);
        }
    }

    private IEnumerator FollowTarget()
    {
        while (isActive)
        {
            // Kiểm tra xem target hiện tại có tồn tại không
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                // Tìm target gần nhất nếu target hiện tại đã chết
                FindNearestTarget();
            }

            // Di chuyển đến vị trí của target hiện tại
            if (currentTarget != null)
            {
                // Tính toán vị trí đích
                Vector3 targetPosition = currentTarget.transform.position + Vector3.up * heightOffset;
                
                // Di chuyển từ từ đến vị trí đích với vận tốc cố định
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    targetPosition, 
                    moveSpeed * Time.deltaTime
                );
                
                // Đảm bảo giữ nguyên rotation x = -90
                transform.rotation = Quaternion.Euler(-90, 0, 0);
            }

            yield return null; // Cập nhật mỗi frame
        }
    }

    private void FindNearestTarget()
    {
        float closestDistance = float.MaxValue;
        EnemyHealth closestEnemy = null;

        // Dùng FindObjectsByType thay vì FindObjectsOfType
        EnemyHealth[] allEnemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        // Cập nhật target mới
        currentTarget = closestEnemy;
    }
}