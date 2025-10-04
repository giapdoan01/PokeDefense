using UnityEngine;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    [Header("Skill Progression System")]
    [SerializeField] private List<SkillData> skillLevels = new List<SkillData>();
    [SerializeField] private int currentSkillLevel = 0;
    
    [Header("Skill Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("🔍 DEBUG")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private float cooldownTimer;
    private bool canCastSkill = false;
    private Transform portalEnd;
    private EnemyHealth currentTarget;
    private Animator pokemonAnimator;
    private GameObject activeSkillInstance;
    
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
            enabled = false;
            return;
        }
        
        if (CurrentSkillData == null || CurrentSkillData.skillPrefab == null)
        {
            enabled = false;
            return;
        }
        
        EnableSkill();
    }

    private void Update()
    {
        if (CurrentSkillData == null) return;
        
        // Cooldown countdown
        if (cooldownTimer > 0) 
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Kiểm tra target còn hợp lệ không
        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                SetIdleAnimation();
            }
            else
            {
                float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distToTarget > CurrentSkillData.baseRange)
                {
                    currentTarget = null;
                    SetIdleAnimation();
                }
            }
        }
        
        // Tìm target mới nếu không còn
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();
            
            if (currentTarget == null)
            {
                SetIdleAnimation();
            }
        }
        
        // Xoay về target và cast skill
        if (currentTarget != null)
        {
            Vector3 direction = currentTarget.transform.position - transform.position;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            if (cooldownTimer <= 0 && canCastSkill)
            {
                CastSkill();
            }
            else
            {
                SetIdleAnimation();
            }
        }
        else
        {
            // Không còn target → hủy skill
            if (activeSkillInstance != null)
            {
                Destroy(activeSkillInstance);
                activeSkillInstance = null;
            }
        }
    }

    public void UpgradeSkill()
    {
        if (!CanUpgrade)
        {
            return;
        }
        
        if (activeSkillInstance != null)
        {
            Destroy(activeSkillInstance);
            activeSkillInstance = null;
        }
        
        int oldLevel = currentSkillLevel;
        currentSkillLevel++;
        cooldownTimer = 0f;
    }

    public void CastSkillEvent()
    {
        if (CurrentSkillData == null || cooldownTimer > 0) return;
        
        CastSkill();
    }

    private void CastSkill()
    {
        if (CurrentSkillData?.skillPrefab == null || currentTarget == null) return;
        
        // ✅ NẾU ĐÃ CÓ SKILL → KHÔNG TẠO MỚI, CHỈ RESET COOLDOWN
        if (activeSkillInstance != null)
        {
            cooldownTimer = CurrentSkillData.baseCooldown;
            return;
        }
        
        // ✅ TẠO SKILL MỚI
        activeSkillInstance = Instantiate(CurrentSkillData.skillPrefab, transform.position, Quaternion.identity);
        
        var skill = activeSkillInstance.GetComponent<ISkill>();
        if (skill != null)
        {
            skill.Initialize(CurrentSkillData.baseDamage, CurrentSkillData.baseRange, currentTarget, pokemonAnimator);
        }
        else
        {
            Destroy(activeSkillInstance);
            activeSkillInstance = null;
            return;
        }
        
        cooldownTimer = CurrentSkillData.baseCooldown;
        
        // ✅ TRIGGER ANIMATION
        if (pokemonAnimator != null)
        {
            pokemonAnimator.SetBool("IsAttacking", true);
            pokemonAnimator.SetTrigger("Skill");
        }
    }

    private EnemyHealth FindNearestEnemy()
    {
        if (portalEnd == null || CurrentSkillData == null) return null;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyHealth nearest = null;
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
                nearest = health;
            }
        }
        
        return nearest;
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
    }

    public void DisableSkill()
    {
        canCastSkill = false;
        
        SetIdleAnimation();
        
        if (activeSkillInstance != null)
        {
            Destroy(activeSkillInstance);
            activeSkillInstance = null;
        }
    }
    
    public void DestroyActiveSkill()
    {
        if (activeSkillInstance != null)
        {
            Destroy(activeSkillInstance);
            activeSkillInstance = null;
        }
    }

    private void DebugLog(string message)
    {
        if (!enableDebugLogs) return;
    }

    private void OnDrawGizmos()
    {
        if (CurrentSkillData == null) return;
        
        Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CurrentSkillData.baseRange);
        
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
        }
    }

    private void OnDestroy()
    {
        if (activeSkillInstance != null)
        {
            Destroy(activeSkillInstance);
        }
    }
}