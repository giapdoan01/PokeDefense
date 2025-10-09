using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    
    [Header("🐢 Slow Effect")]
    [SerializeField] private float currentSpeedMultiplier = 1f;
    
    private float baseSpeed; // Tốc độ gốc
    private Transform target;
    private int wavepointIndex = 0;
    
    // ✅ TRACKING SLOW EFFECTS (cho phép nhiều skills slow cùng lúc)
    private Dictionary<WartortleSkill, float> activeSlowEffects = new Dictionary<WartortleSkill, float>();
    
    // ✅ PROPERTY ĐỂ SkillController ĐỌC WAYPOINT INDEX
    public int CurrentWaypointIndex => wavepointIndex;
    public int TotalWaypoints => ForwardPoints.points != null ? ForwardPoints.points.Length : 0;
    
    // ✅ TÍNH % TIẾN ĐỘ ĐẾN CỬA (0-100%)
    public float ProgressPercent
    {
        get
        {
            if (TotalWaypoints == 0) return 0f;
            
            float baseProgress = (float)wavepointIndex / TotalWaypoints * 100f;
            
            // Thêm khoảng cách đến waypoint tiếp theo để chính xác hơn
            if (target != null && wavepointIndex < TotalWaypoints)
            {
                float distToNextWaypoint = Vector3.Distance(
                    new Vector3(transform.position.x, 0, transform.position.z),
                    new Vector3(target.position.x, 0, target.position.z)
                );
                
                // Ước lượng khoảng cách giữa các waypoint
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

    void Start()
    {
        baseSpeed = speed; // Lưu tốc độ gốc
        target = ForwardPoints.points[0];
    }

    void Update()
    {
        if (target == null) return;
        
        // Giữ nguyên Y hiện tại, chỉ tính khoảng cách XZ
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        // Nếu có hướng di chuyển thì xoay enemy về phía đó
        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        // ✅ DI CHUYỂN VỚI TỐC ĐỘ ĐÃ BỊ SLOW
        float currentSpeed = baseSpeed * currentSpeedMultiplier;
        transform.Translate(dir.normalized * currentSpeed * Time.deltaTime, Space.World);

        // Check khoảng cách tới waypoint (XZ)
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                             new Vector3(target.position.x, 0, target.position.z)) <= 0.2f)
        {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        if (wavepointIndex >= ForwardPoints.points.Length - 1)
        {
            Destroy(gameObject);
            return;
        }
        wavepointIndex++;
        target = ForwardPoints.points[wavepointIndex];
    }
    
    // ✅ ÁP DỤNG SLOW EFFECT
    public void ApplySlowEffect(WartortleSkill skill, float slowPercent)
    {
        if (!activeSlowEffects.ContainsKey(skill))
        {
            activeSlowEffects[skill] = slowPercent;
            UpdateSpeedMultiplier();
        }
    }
    
    // ✅ XÓA SLOW EFFECT
    public void RemoveSlowEffect(WartortleSkill skill)
    {
        if (activeSlowEffects.ContainsKey(skill))
        {
            activeSlowEffects.Remove(skill);
            UpdateSpeedMultiplier();
        }
    }
    
    // ✅ CẬP NHẬT TỐC ĐỘ (lấy slow effect mạnh nhất)
    private void UpdateSpeedMultiplier()
    {
        if (activeSlowEffects.Count == 0)
        {
            currentSpeedMultiplier = 1f; // Tốc độ bình thường
        }
        else
        {
            // Lấy slow effect mạnh nhất (giá trị nhỏ nhất)
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
        
        Debug.Log($"🐢 {gameObject.name} speed: {currentSpeedMultiplier * 100}% ({baseSpeed * currentSpeedMultiplier:F1} units/s)");
    }
}
