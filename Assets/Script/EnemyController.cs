using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f;
    private Transform target;
    private int wavepointIndex = 0;

    void Start()
    {
        target = ForwardPoints.points[0];
    }

    void Update()
    {
        // Giữ nguyên Y hiện tại, chỉ tính khoảng cách XZ
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;  // bỏ phần thay đổi theo Y

        // Nếu có hướng di chuyển thì xoay enemy về phía đó
        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        // Di chuyển enemy
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

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
}
