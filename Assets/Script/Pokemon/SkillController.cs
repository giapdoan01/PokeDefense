using UnityEngine;
using System.Linq;
using System.Collections;

public class SkillController : MonoBehaviour
{
    [SerializeField] private SkillData skillData;
    
    private float skillDamage;
    private float skillCooldown;
    private float skillRange;
    private float cooldownTimer;
    private bool canCastSkill = false; // Biến để kiểm soát việc có thể cast skill hay không
    
    [SerializeField] private float rotationSpeed = 10f; // Tốc độ xoay của Pokemon
    private Transform portalEnd;
    private EnemyHealth currentTarget; // Lưu mục tiêu hiện tại
    private Animator animator; // Tham chiếu đến Animator

    private void Start()
    {
        if (skillData != null)
        {
            skillDamage = skillData.baseDamage;
            skillCooldown = skillData.baseCooldown;
            skillRange = skillData.baseRange;
        }

        GameObject pe = GameObject.FindGameObjectWithTag("PE");
        if (pe != null) portalEnd = pe.transform;
        
        // Lấy Animator component từ GameObject
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (skillData == null || portalEnd == null) return;
        
        // Liên tục tìm kẻ địch gần nhất và xoay về phía chúng
        currentTarget = FindTarget();
        if (currentTarget != null)
        {
            FaceTarget(currentTarget.transform);
        }
        
        // Xử lý cooldown
        cooldownTimer -= Time.deltaTime;
        
        // Khi hết cooldown và có target, set có thể cast skill và trigger animation
        if (cooldownTimer <= 0f && currentTarget != null && !canCastSkill)
        {
            canCastSkill = true;
            // Trigger animation attack - animation sẽ gọi CastSkillEvent thông qua animation event
            if (animator != null)
            {
                animator.SetBool("IsAttack", true);
            }
        }
        else if (currentTarget == null && animator != null && canCastSkill)
        {
            // Nếu không có target, dừng animation tấn công
            animator.SetBool("IsAttack", false);
            canCastSkill = false;
        }
    }

    // Phương thức này sẽ được gọi từ animation event
    public void CastSkillEvent()
    {
        // Chỉ cast skill khi có thể và có target
        if (canCastSkill && currentTarget != null)
        {
            CastSkill();
            canCastSkill = false;
            cooldownTimer = skillCooldown;
        }
    }

    // Phương thức này sẽ được gọi từ animation event khi animation attack kết thúc
    public void OnAttackAnimationEnd()
    {
        if (animator != null)
        {
            animator.SetBool("IsAttack", false);
        }
    }

    private void CastSkill()
    {
        if (currentTarget == null) return;

        // Spawn skill tại vị trí Pokemon
        GameObject skillObj = Instantiate(skillData.skillPrefab, transform.position, Quaternion.identity);

        var skillComp = skillObj.GetComponent<ISkill>();
        if (skillComp != null)
        {
            // Truyền target và các thông số khác
            skillComp.Initialize(skillDamage, skillRange, currentTarget, null);
        }
    }

    // Hàm xoay mặt Pokemon về phía mục tiêu
    private void FaceTarget(Transform target)
    {
        if (target == null) return;
        
        // Tính vector hướng từ Pokemon đến kẻ địch (chỉ trên mặt phẳng XZ)
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Bỏ qua trục Y để chỉ xoay theo mặt phẳng ngang
        
        // Chỉ xoay nếu vector hướng không phải vector 0
        if (directionToTarget != Vector3.zero)
        {
            // Tính góc quay để nhìn về phía mục tiêu
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // Xoay Pokemon từ từ về phía mục tiêu
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private EnemyHealth FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        var inRange = enemies
            .Select(e => e.GetComponent<EnemyHealth>())
            .Where(eh => eh != null && Vector3.Distance(transform.position, eh.transform.position) <= skillRange);

        if (!inRange.Any()) return null;

        return inRange
            .OrderBy(eh => Vector3.Distance(eh.transform.position, portalEnd.position))
            .FirstOrDefault();
    }

    // Upgrade API
    public void UpgradeDamage(float amount) => skillDamage += amount;
    public void ReduceCooldown(float amount) => skillCooldown = Mathf.Max(0.1f, skillCooldown - amount);
    public void IncreaseRange(float amount) => skillRange += amount;
}