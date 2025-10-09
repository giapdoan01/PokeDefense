using UnityEngine;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    [Header("Skill Progression System")]
    [SerializeField] private List<SkillData> skillLevels = new List<SkillData>();
    [SerializeField] private int currentSkillLevel = 0;
    
    [Header("Skill Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("🎯 Target Priority")]
    [SerializeField] private bool prioritizeByWaypoint = true; // Toggle giữa 2 chế độ
    
    [Header("DEBUG")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private float cooldownTimer;
    private bool canCastSkill = false;
    private Transform portalEnd;
    private EnemyHealth currentTarget;
    private Animator pokemonAnimator;
    
    public SkillData CurrentSkillData => (skillLevels != null && skillLevels.Count > 0 && currentSkillLevel < skillLevels.Count) 
        ? skillLevels[currentSkillLevel] : null;
    
    public bool CanUpgrade => currentSkillLevel < skillLevels.Count - 1;
    public int CurrentLevel => currentSkillLevel + 1;
    public int MaxLevel => skillLevels.Count;
    public float CurrentCooldown => cooldownTimer;

    private void Start()
    {
        portalEnd = GameObject.FindGameObjectWithTag("PE")?.transform;
        pokemonAnimator = GetComponent<Animator>();
        
        if (skillLevels == null || skillLevels.Count == 0)
        {
            Debug.LogError("❌ No skill levels assigned!");
            enabled = false;
            return;
        }
        
        if (CurrentSkillData == null || CurrentSkillData.skillPrefab == null)
        {
            Debug.LogError("❌ Current skill data or prefab is null!");
            enabled = false;
            return;
        }
        
        EnableSkill();
        DebugLog($"✅ SkillController initialized - Level {CurrentLevel}/{MaxLevel}");
    }

    private void Update()
    {
        if (CurrentSkillData == null) return;
        
        // Cooldown timer
        if (cooldownTimer > 0) 
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Check target validity
        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy)
            {
                DebugLog($"❌ Target {currentTarget.name} destroyed");
                currentTarget = null;
                SetIdleAnimation();
            }
            else
            {
                float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distToTarget > CurrentSkillData.baseRange)
                {
                    DebugLog($"❌ Target {currentTarget.name} out of range ({distToTarget:F1}m > {CurrentSkillData.baseRange}m)");
                    currentTarget = null;
                    SetIdleAnimation();
                }
            }
        }
        
        // Find new target
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();
            
            if (currentTarget == null)
            {
                SetIdleAnimation();
            }
        }
        
        // Attack target
        if (currentTarget != null)
        {
            // Rotate to target
            Vector3 direction = currentTarget.transform.position - transform.position;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Cast skill
            if (cooldownTimer <= 0 && canCastSkill)
            {
                if (pokemonAnimator != null && !pokemonAnimator.GetBool("IsAttacking"))
                {
                    pokemonAnimator.SetBool("IsAttacking", true);
                }
            }
            else if (cooldownTimer > 0)
            {
                SetIdleAnimation();
            }
        }
    }

    public void UpgradeSkill()
    {
        if (!CanUpgrade)
        {
            Debug.LogWarning("⚠️ Already at max level!");
            return;
        }
        
        int oldLevel = currentSkillLevel;
        currentSkillLevel++;
        cooldownTimer = 0f;
        
        DebugLog($"⬆️ Skill upgraded: Level {oldLevel + 1} → {CurrentLevel}");
    }

    public void CastSkillEvent()
    {
        if (CurrentSkillData == null || cooldownTimer > 0) return;
        
        CastSkill();
    }

    private void CastSkill()
    {
        if (CurrentSkillData?.skillPrefab == null || currentTarget == null) return;
        
        GameObject newSkill = Instantiate(
            CurrentSkillData.skillPrefab, 
            transform.position, 
            Quaternion.identity
        );
        
        newSkill.name = $"Skill_{Time.time:F1}s";
        
        var skill = newSkill.GetComponent<ISkill>();
        if (skill != null)
        {
            skill.Initialize(
                CurrentSkillData.baseDamage, 
                CurrentSkillData.baseRange, 
                currentTarget, 
                pokemonAnimator
            );
            
            DebugLog($"🌊 Skill spawned: {newSkill.name} → Target: {currentTarget.name}");
        }
        else
        {
            Debug.LogError("❌ NO ISkill COMPONENT!");
            Destroy(newSkill);
            return;
        }
        
        cooldownTimer = CurrentSkillData.baseCooldown;
    }

    // ✅ LOGIC TÌM TARGET MỚI (DỰA VÀO WAYPOINT)
    private EnemyHealth FindNearestEnemy()
    {
        if (CurrentSkillData == null) return null;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyHealth bestTarget = null;
        
        if (prioritizeByWaypoint)
        {
            // ✅ CHỌN ENEMY GẦN CỬA NHẤT (DỰA VÀO WAYPOINT INDEX)
            int highestWaypointIndex = -1;
            float highestProgress = -1f;
            float closestDistAtSameProgress = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (!enemy.activeInHierarchy) continue;
                
                // Kiểm tra trong range
                float distToPlayer = Vector3.Distance(transform.position, enemy.transform.position);
                if (distToPlayer > CurrentSkillData.baseRange) continue;
                
                var health = enemy.GetComponent<EnemyHealth>();
                if (health == null) continue;
                
                var controller = enemy.GetComponent<EnemyController>();
                if (controller == null) continue;
                
                int waypointIndex = controller.CurrentWaypointIndex;
                float progress = controller.ProgressPercent;
                
                // Ưu tiên enemy có progress cao hơn (gần cửa hơn)
                if (waypointIndex > highestWaypointIndex || 
                    (waypointIndex == highestWaypointIndex && progress > highestProgress))
                {
                    highestWaypointIndex = waypointIndex;
                    highestProgress = progress;
                    closestDistAtSameProgress = distToPlayer;
                    bestTarget = health;
                    
                    DebugLog($"🎯 New priority target: {enemy.name} (WP {waypointIndex}, Progress {progress:F1}%)");
                }
                // Nếu cùng progress, chọn enemy gần pokemon hơn
                else if (waypointIndex == highestWaypointIndex && 
                         Mathf.Abs(progress - highestProgress) < 1f && 
                         distToPlayer < closestDistAtSameProgress)
                {
                    closestDistAtSameProgress = distToPlayer;
                    bestTarget = health;
                    
                    DebugLog($"🎯 Closer target at same progress: {enemy.name} (Dist {distToPlayer:F1}m)");
                }
            }
        }
        else
        {
            // ❌ CHỌN ENEMY GẦN PE NHẤT (LOGIC CŨ)
            if (portalEnd == null)
            {
                Debug.LogWarning("⚠️ Portal End not found! Using waypoint mode instead.");
                prioritizeByWaypoint = true;
                return FindNearestEnemy();
            }
            
            float closestDistToPortal = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (!enemy.activeInHierarchy) continue;
                
                var health = enemy.GetComponent<EnemyHealth>();
                if (health == null) continue;
                
                float distToPlayer = Vector3.Distance(transform.position, enemy.transform.position);
                if (distToPlayer > CurrentSkillData.baseRange) continue;
                
                float distToPortal = Vector3.Distance(enemy.transform.position, portalEnd.position);
                if (distToPortal < closestDistToPortal)
                {
                    closestDistToPortal = distToPortal;
                    bestTarget = health;
                }
            }
        }
        
        return bestTarget;
    }

    private void SetIdleAnimation()
    {
        if (pokemonAnimator != null)
        {
            pokemonAnimator.SetBool("IsAttacking", false);
        }
    }

    public void EnableSkill()
    {
        canCastSkill = true;
        DebugLog("✅ Skill enabled");
    }

    public void DisableSkill()
    {
        canCastSkill = false;
        SetIdleAnimation();
        DebugLog("❌ Skill disabled");
    }
    
    public void DestroyActiveSkill()
    {
        // Skill sẽ tự destroy theo duration
    }

    private void DebugLog(string message)
    {
        if (!enableDebugLogs) return;
        Debug.Log($"[{gameObject.name}] {message}");
    }

    private void OnDrawGizmos()
    {
        if (CurrentSkillData == null) return;
        
        // Vẽ range
        Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CurrentSkillData.baseRange);
        
        // Vẽ line đến target
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            if (dist <= CurrentSkillData.baseRange)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
            
            #if UNITY_EDITOR
            // ✅ HIỂN THỊ WAYPOINT INDEX VÀ PROGRESS
            var controller = currentTarget.GetComponent<EnemyController>();
            if (controller != null)
            {
                UnityEditor.Handles.Label(
                    currentTarget.transform.position + Vector3.up * 2f,
                    $"WP: {controller.CurrentWaypointIndex}/{controller.TotalWaypoints}\n" +
                    $"Progress: {controller.ProgressPercent:F1}%\n" +
                    $"Dist: {dist:F1}m"
                );
            }
            #endif
        }
    }
}
