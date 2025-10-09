using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharizardSkillController : SkillController
{
    [SerializeField] private int maxTargets = 3;
    
    [Header("🎯 Rotation")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("🔥 DEBUG")]
    [SerializeField] private bool showDebugLogs = true;
    
    private float nextCastTime = 0f;
    private List<EnemyHealth> pendingTargets = new List<EnemyHealth>();
    private Animator pokemonAnimator;
    private EnemyHealth farthestTarget; // Target xa nhất để xoay mặt
    
    private void Start()
    {
        pokemonAnimator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (CurrentSkillData == null) return;
        if (pokemonAnimator == null) return;
        
        // ✅ XOAY VỀ TARGET XA NHẤT (nếu có)
        if (farthestTarget != null && farthestTarget.gameObject.activeInHierarchy)
        {
            RotateTowardsTarget(farthestTarget.transform.position);
        }
        
        // Cooldown
        if (Time.time < nextCastTime) return;
        
        // Tìm enemy trong range
        List<EnemyHealth> enemiesInRange = FindEnemiesInRange();
        
        if (enemiesInRange.Count > 0)
        {
            // Lưu target và tìm target xa nhất
            pendingTargets = enemiesInRange;
            farthestTarget = GetFarthestTarget(enemiesInRange);
            
            // ✅ SET BOOL ISATTACKING = TRUE
            if (!pokemonAnimator.GetBool("IsAttacking"))
            {
                pokemonAnimator.SetBool("IsAttacking", true);
            }
            
            nextCastTime = Time.time + CurrentSkillData.baseCooldown;
        }
        else
        {
            // ✅ KHÔNG CÓ TARGET → RESET
            farthestTarget = null;
            
            if (pokemonAnimator.GetBool("IsAttacking"))
            {
                pokemonAnimator.SetBool("IsAttacking", false);
            }
        }
    }
    
    // ✅ XOAY VỀ TARGET
    private void RotateTowardsTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Chỉ xoay theo trục Y (không nghiêng lên/xuống)
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
    
    // ✅ TÌM TARGET XA NHẤT
    private EnemyHealth GetFarthestTarget(List<EnemyHealth> enemies)
    {
        if (enemies == null || enemies.Count == 0) return null;
        
        return enemies
            .OrderByDescending(e => Vector3.Distance(transform.position, e.transform.position))
            .FirstOrDefault();
    }
    
    // ✅ METHOD NÀY ĐƯỢC GỌI TỪ ANIMATION EVENT
    public new void CastSkillEvent()
    {
        if (pendingTargets.Count > 0)
        {
            CastMultipleSkills(pendingTargets);
            pendingTargets.Clear();
        }
        
        // ✅ SAU KHI CAST XONG → SET IDLE
        if (pokemonAnimator != null)
        {
            pokemonAnimator.SetBool("IsAttacking", false);
        }
        
        // Reset target
        farthestTarget = null;
    }
    
    private List<EnemyHealth> FindEnemiesInRange()
    {
        List<EnemyHealth> enemies = new List<EnemyHealth>();
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in allEnemies)
        {
            if (!enemy.activeInHierarchy) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance <= CurrentSkillData.baseRange)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemies.Add(enemyHealth);
                }
            }
        }
        
        // ✅ SẮP XẾP THEO KHOẢNG CÁCH GẦN → XA
        return enemies
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .ToList();
    }
    
    private void CastMultipleSkills(List<EnemyHealth> enemies)
    {
        int targetCount = Mathf.Min(enemies.Count, maxTargets);
        
        for (int i = 0; i < targetCount; i++)
        {
            EnemyHealth targetEnemy = enemies[i];
            
            // ✅ KIỂM TRA TARGET CÒN SỐNG KHÔNG
            if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
            {
                continue;
            }
            
            Vector3 enemyPos = targetEnemy.transform.position;
            
            // Spawn skill
            GameObject skillInstance = Instantiate(
                CurrentSkillData.skillPrefab,
                enemyPos,
                Quaternion.identity
            );
            
            skillInstance.name = $"CharizardSkill_{i+1}_{targetEnemy.name}";
            
            // Initialize
            ISkill skill = skillInstance.GetComponent<ISkill>();
            if (skill != null)
            {
                skill.Initialize(
                    CurrentSkillData.baseDamage,
                    CurrentSkillData.baseRange,
                    targetEnemy,
                    pokemonAnimator
                );
            }
            else
            {
                Destroy(skillInstance);
            }
        }
    }
    
    // ✅ DEBUG VISUALIZATION
    private void OnDrawGizmos()
    {
        if (CurrentSkillData == null) return;
        
        // Vẽ range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CurrentSkillData.baseRange);
        
        // Vẽ line đến farthest target
        if (farthestTarget != null && farthestTarget.gameObject.activeInHierarchy)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, farthestTarget.transform.position);
            Gizmos.DrawWireSphere(farthestTarget.transform.position, 0.8f);
        }
        
        // Vẽ line đến tất cả pending targets
        if (pendingTargets != null && pendingTargets.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (var target in pendingTargets)
            {
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    Gizmos.DrawLine(transform.position, target.transform.position);
                    Gizmos.DrawWireSphere(target.transform.position, 0.5f);
                }
            }
        }
    }
}