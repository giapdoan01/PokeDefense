using UnityEngine;
using System.Linq;

public class SkillController : MonoBehaviour
{
    [SerializeField] private SkillData skillData;

    private float skillDamage;
    private float skillCooldown;
    private float skillRange;
    private float cooldownTimer;

    private Transform portalEnd; // portal end trong map

    private void Start()
    {
        if (skillData != null)
        {
            skillDamage = skillData.baseDamage;
            skillCooldown = skillData.baseCooldown;
            skillRange = skillData.baseRange;
        }

        // Tìm portalEnd theo tag "PE"
        GameObject pe = GameObject.FindGameObjectWithTag("PE");
        if (pe != null)
        {
            portalEnd = pe.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy PortalEnd nào có tag 'PE' trong scene!");
        }
    }

    private void Update()
    {
        if (skillData == null || portalEnd == null) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            TryCastSkill();
            cooldownTimer = skillCooldown;
        }
    }

    private void TryCastSkill()
    {
        EnemyController target = FindTarget();
        if (target == null) return;

        // Spawn skill tại vị trí enemy
        GameObject skillObj = Instantiate(skillData.skillPrefab, target.transform.position, Quaternion.identity);

        // Gửi damage vào skill nếu prefab có script xử lý
        var skillComp = skillObj.GetComponent<ISkill>();
        if (skillComp != null)
        {
            skillComp.Initialize(skillDamage);
        }
    }

    private EnemyController FindTarget()
    {
        // Tìm tất cả enemy theo tag
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyObjects.Length == 0) return null;

        // Lọc theo khoảng cách trong range
        var inRange = enemyObjects
            .Select(e => e.GetComponent<EnemyController>())
            .Where(e => e != null && Vector3.Distance(transform.position, e.transform.position) <= skillRange);

        if (!inRange.Any()) return null;

        // Chọn enemy gần portalEnd nhất
        return inRange
            .OrderBy(e => Vector3.Distance(e.transform.position, portalEnd.position))
            .FirstOrDefault();
    }

    // Upgrade API
    public void UpgradeDamage(float amount) => skillDamage += amount;
    public void ReduceCooldown(float amount) => skillCooldown = Mathf.Max(0.1f, skillCooldown - amount);
    public void IncreaseRange(float amount) => skillRange += amount;
}
