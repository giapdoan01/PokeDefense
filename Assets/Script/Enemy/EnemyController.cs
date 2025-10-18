using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{

    [Header("Movement")]
    private float baseSpeed;

    [Header("🐢 Slow Effect")]
    [SerializeField] private float currentSpeedMultiplier = 1f;

    // Tham chiếu
    private Transform target;
    private int wavepointIndex = 0;
    private EnemyHealth enemyHealth;

    // Dictionary lưu trữ các hiệu ứng làm chậm theo ID
    private Dictionary<int, float> activeSlowEffects = new Dictionary<int, float>();

    public int CurrentWaypointIndex => wavepointIndex;
    public int TotalWaypoints => ForwardPoints.points != null ? ForwardPoints.points.Length : 0;

    public float ProgressPercent
    {
        get
        {
            if (TotalWaypoints == 0) return 0f;

            float baseProgress = (float)wavepointIndex / TotalWaypoints * 100f;

            if (target != null && wavepointIndex < TotalWaypoints)
            {
                float distToNextWaypoint = Vector3.Distance(
                    new Vector3(transform.position.x, 0, transform.position.z),
                    new Vector3(target.position.x, 0, target.position.z)
                );

                float waypointSpacing = 10f;
                if (wavepointIndex > 0 && wavepointIndex < TotalWaypoints)
                {
                    waypointSpacing = Vector3.Distance(
                        ForwardPoints.points[wavepointIndex - 1].position,
                        ForwardPoints.points[wavepointIndex].position
                    );
                }

                float subProgress = Mathf.Clamp01(1f - (distToNextWaypoint / waypointSpacing)) / TotalWaypoints * 100f;
                return baseProgress + subProgress;
            }

            return baseProgress;
        }
    }

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        wavepointIndex = 0;

        activeSlowEffects.Clear();
        currentSpeedMultiplier = 1f;

        if (ForwardPoints.points != null && ForwardPoints.points.Length > 0)
        {
            target = ForwardPoints.points[0];
        }

        InitializeSpeed();
    }

    private void Update()
    {
        MoveTowardsTarget();
    }

    private void InitializeSpeed()
    {
        if (enemyHealth != null && enemyHealth.Stats != null)
        {
            baseSpeed = enemyHealth.Stats.enemySpeed;
        }
        else
        {
            baseSpeed = 3f;
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        float currentSpeed = baseSpeed * currentSpeedMultiplier;
        transform.Translate(dir.normalized * currentSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(target.position.x, 0, target.position.z)) <= 0.2f)
        {
            GetNextWaypoint();
        }
    }

    private void GetNextWaypoint()
    {
        if (wavepointIndex >= ForwardPoints.points.Length - 1)
        {
            OnReachedEndpoint();
            return;
        }

        wavepointIndex++;
        target = ForwardPoints.points[wavepointIndex];
    }

    private void OnReachedEndpoint()
    {
        if (GameBattleManager.Instance != null)
        {
            GameBattleManager.Instance.OnEnemyEscaped();
        }
        Destroy(gameObject);
    }

    // Cập nhật tốc độ từ EnemyStats
    public void UpdateFromStats(EnemyStats stats)
    {
        if (stats != null)
        {
            baseSpeed = stats.enemySpeed;
        }
    }

    // Áp dụng hiệu ứng làm chậm
    public void ApplySlowEffect(ISlowEffectSource source)
    {
        if (source == null) return;

        if (!activeSlowEffects.ContainsKey(source.EffectID))
        {
            activeSlowEffects[source.EffectID] = source.SlowPercent;
            UpdateSpeedMultiplier();
        }
    }

    // Xóa hiệu ứng làm chậm
    public void RemoveSlowEffect(ISlowEffectSource source)
    {
        if (source == null) return;

        if (activeSlowEffects.ContainsKey(source.EffectID))
        {
            activeSlowEffects.Remove(source.EffectID);
            UpdateSpeedMultiplier();
        }
    }

    // Cập nhật hệ số làm chậm tốc độ (lấy hiệu ứng mạnh nhất)
    private void UpdateSpeedMultiplier()
    {
        if (activeSlowEffects.Count == 0)
        {
            currentSpeedMultiplier = 1f;
        }
        else
        {
            float lowestMultiplier = 1f;
            foreach (var slowPercent in activeSlowEffects.Values)
            {
                if (slowPercent < lowestMultiplier)
                {
                    lowestMultiplier = slowPercent;
                }
            }
            currentSpeedMultiplier = lowestMultiplier;
        }
    }
}