using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VenusaurSkill : MonoBehaviour, ISkill
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float searchRadius = 2f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float damageDelay = 0.1f;
    
    private float damage;
    private float range;
    private Vector3 originalPosition;
    private Transform venusaurTransform;
    private HashSet<EnemyHealth> hitTargets = new HashSet<EnemyHealth>();
    private EnemyHealth currentTarget;
    private bool isReturning = false;
    private bool isActive = true;
    private List<EnemyHealth> enemiesInScene = new List<EnemyHealth>();
    private int targetsHit = 0;

    public void Initialize(float damage, float range, EnemyHealth target = null, Animator pokemonAnimator = null)
    {
        this.damage = damage;
        this.range = range;
        this.venusaurTransform = transform.parent;
        this.originalPosition = transform.position;
        
        // Tách khỏi parent để di chuyển độc lập
        transform.parent = null;
        
        // Thiết lập position y = 0.4
        Vector3 newPosition = transform.position;
        newPosition.y = 0.4f;
        transform.position = newPosition;
        
        // Thiết lập rotation với x = -90
        transform.rotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        
        // Lấy tất cả enemy trong scene
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyObjects)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null && health.gameObject.activeInHierarchy)
            {
                enemiesInScene.Add(health);
            }
        }
        
        // Bắt đầu với target được cung cấp
        if (target != null && target.gameObject.activeInHierarchy)
        {
            currentTarget = target;
            hitTargets.Add(target);
        }
        else
        {
            // Tìm target gần nhất nếu không có target được cung cấp
            currentTarget = FindNearestEnemy();
        }
        
        // Bắt đầu di chuyển
        if (currentTarget != null)
        {
            StartCoroutine(MoveToTargets());
        }
        else
        {
            // Không có target, quay về ngay
            isReturning = true;
            StartCoroutine(ReturnToVenusaur());
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Nếu target hiện tại không còn hoạt động nữa, tìm target mới
        if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
        {
            FindNextTarget();
        }
    }
    
    private EnemyHealth FindNearestEnemy()
    {
        float closestDistance = float.MaxValue;
        EnemyHealth closest = null;
        
        foreach (EnemyHealth enemy in enemiesInScene)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || hitTargets.Contains(enemy))
                continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= range)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }
        
        return closest;
    }
    
    private void FindNextTarget()
    {
        List<EnemyHealth> nearbyEnemies = new List<EnemyHealth>();
        
        foreach (EnemyHealth enemy in enemiesInScene)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || hitTargets.Contains(enemy))
                continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= searchRadius)
            {
                nearbyEnemies.Add(enemy);
            }
        }
        
        if (nearbyEnemies.Count > 0)
        {
            // Sắp xếp theo khoảng cách
            nearbyEnemies.Sort((a, b) => 
                Vector3.Distance(transform.position, a.transform.position)
                .CompareTo(Vector3.Distance(transform.position, b.transform.position))
            );
            
            // Lấy enemy gần nhất
            currentTarget = nearbyEnemies[0];
            hitTargets.Add(currentTarget);
        }
        else
        {
            // Không tìm thấy target nào trong bán kính, bắt đầu quay về
            currentTarget = null;
            isReturning = true;
            StartCoroutine(ReturnToVenusaur());
        }
    }
    
    private IEnumerator MoveToTargets()
    {
        while (isActive && !isReturning && targetsHit < maxTargets)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                FindNextTarget();
                if (currentTarget == null)
                {
                    // Không còn target, bắt đầu quay về
                    break;
                }
            }
            
            // Di chuyển đến target - so sánh chỉ trên mặt phẳng XZ
            while (GetHorizontalDistance(transform.position, currentTarget.transform.position) > 0.5f)
            {
                if (!isActive || currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
                {
                    break;
                }
                
                // Tính toán vị trí mới, giữ y = 0.4
                Vector3 targetPos = currentTarget.transform.position;
                targetPos.y = 0.4f;
                
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    targetPos, 
                    moveSpeed * Time.deltaTime
                );
                
                // Nhìn vào hướng di chuyển, giữ góc X là -90
                Vector3 direction = (targetPos - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Euler(-90f, lookRotation.eulerAngles.y, lookRotation.eulerAngles.z);
                }
                
                yield return null;
            }
            
            // Đã đến target, gây sát thương
            if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            {
                // Gây damage
                currentTarget.TakeDamage(damage);
                targetsHit++;
                
                // Debug
                Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red, 1.0f);
                Debug.Log($"VenusaurSkill hit target {currentTarget.name}. Targets hit: {targetsHit}/{maxTargets}");
                
                // Delay nhỏ sau khi hit
                yield return new WaitForSeconds(damageDelay);
                
                // Tìm target tiếp theo
                FindNextTarget();
            }
        }
        
        // Kết thúc chuỗi targets hoặc đã đạt max, bắt đầu quay về
        isReturning = true;
        StartCoroutine(ReturnToVenusaur());
    }
    
    private IEnumerator ReturnToVenusaur()
    {
        Vector3 targetPosition = venusaurTransform != null ? venusaurTransform.position : originalPosition;
        targetPosition.y = 0.4f; // Đảm bảo y = 0.4 khi quay về
        
        while (isActive && GetHorizontalDistance(transform.position, targetPosition) > 0.5f)
        {
            // Cập nhật vị trí đích nếu Venusaur di chuyển
            if (venusaurTransform != null)
            {
                targetPosition = venusaurTransform.position;
                targetPosition.y = 0.4f; // Đảm bảo y = 0.4
            }
            
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime * 1.5f  // Tăng tốc độ khi quay về
            );
            
            // Nhìn vào hướng di chuyển, giữ góc X là -90
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Euler(-90f, lookRotation.eulerAngles.y, lookRotation.eulerAngles.z);
            }
            
            yield return null;
        }
        
        // Đã quay về Venusaur, hủy skill
        Debug.Log($"VenusaurSkill returned to Venusaur after hitting {targetsHit} targets");
        Destroy(gameObject);
    }
    
    // Phương thức tính khoảng cách chỉ trên mặt phẳng XZ (bỏ qua Y)
    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector2 a2D = new Vector2(a.x, a.z);
        Vector2 b2D = new Vector2(b.x, b.z);
        return Vector2.Distance(a2D, b2D);
    }
    
    public void DeactivateSkill()
    {
        isActive = false;
        Destroy(gameObject);
    }
    
    private void OnDrawGizmos()
    {
        // Hiển thị bán kính tìm kiếm
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
        
        // Hiển thị đường đến target hiện tại
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}