using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    #region Variables

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Wave Control")]
    [SerializeField] private float additionalDelayBetweenEnemies = 0f; // Thêm delay giữa các enemy nếu muốn
    
    [Header("Events")]
    public UnityEvent onWaveStart;
    public UnityEvent onWaveComplete;
    
    // Cache danh sách enemy đang active từ wave hiện tại
    private List<GameObject> activeEnemiesInWave = new List<GameObject>();
    
    #endregion

    #region Public Methods
    
    /// <summary>
    /// Spawn một wave với cấu hình từ WaveConfig
    /// </summary>
    /// <param name="wave">Cấu hình wave</param>
    /// <param name="roundMultiplier">Hệ số nhân round ảnh hưởng đến máu quái</param>
    /// <param name="waitForCompletion">Có đợi tất cả quái bị tiêu diệt không</param>
    public IEnumerator SpawnWave(WaveConfig wave, int roundMultiplier, bool waitForCompletion = true)
    {
        if (wave == null)
        {
            Debug.LogError("WaveSpawner: Wave config is null!");
            yield break;
        }
        
        // Thông báo wave bắt đầu
        onWaveStart?.Invoke();
        activeEnemiesInWave.Clear();
        
        // Spawn từng loại enemy trong wave
        foreach (var enemyStats in wave.enemies)
        {
            if (enemyStats == null || enemyStats.enemyPrefab == null) 
            {
                Debug.LogWarning("WaveSpawner: Skipping null enemy stats or prefab in wave.");
                continue;
            }
            
            // Spawn số lượng enemy được chỉ định
            for (int i = 0; i < enemyStats.spawnCount; i++)
            {
                // Spawn enemy từ object pool
                GameObject enemy = SpawnEnemy(enemyStats, roundMultiplier);
                
                if (enemy != null)
                {
                    activeEnemiesInWave.Add(enemy);
                }
                
                // Chờ khoảng thời gian giữa các lần spawn
                float spawnDelay = enemyStats.timeBetweenSpawns + additionalDelayBetweenEnemies;
                if (spawnDelay > 0 && i < enemyStats.spawnCount - 1) // Chỉ delay giữa các enemy
                {
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
        }
        
        // Nếu yêu cầu đợi hoàn thành (tất cả quái bị tiêu diệt), thì đợi
        if (waitForCompletion)
        {
            // Chờ đến khi tất cả enemy trong wave bị tiêu diệt
            yield return StartCoroutine(WaitForWaveComplete());
            
            // Thông báo wave đã hoàn thành
            onWaveComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Spawn một enemy riêng lẻ với stats được chỉ định
    /// </summary>
    public GameObject SpawnSingleEnemy(EnemyStats enemyStats, int roundMultiplier)
    {
        return SpawnEnemy(enemyStats, roundMultiplier);
    }
    
    #endregion

    #region Private Methods
    
    private GameObject SpawnEnemy(EnemyStats enemyStats, int roundMultiplier)
    {
        // Kiểm tra các thông số cần thiết
        if (enemyStats == null || enemyStats.enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("WaveSpawner: Invalid enemy stats or spawn point");
            return null;
        }
        
        // Lấy enemy từ object pool
        GameObject enemy = ObjectPool.Instance.GetFromPool(enemyStats.enemyPrefab);
        
        if (enemy == null)
        {
            Debug.LogError($"WaveSpawner: Failed to get enemy from pool: {enemyStats.enemyPrefab.name}");
            return null;
        }
        
        // Đặt vị trí spawn
        enemy.transform.position = spawnPoint.position;
        
        // Cấu hình enemy với stats
        ConfigureEnemy(enemy, enemyStats, roundMultiplier);
        
        return enemy;
    }
    
    private void ConfigureEnemy(GameObject enemy, EnemyStats enemyStats, int roundMultiplier)
    {
        // Áp dụng EnemyStats và roundMultiplier
        EnemyHealth healthComponent = enemy.GetComponent<EnemyHealth>();
        if (healthComponent != null)
        {
            // Đặt stats trước, sau đó áp dụng round multiplier
            healthComponent.SetStats(enemyStats);
            healthComponent.ApplyRoundMultiplier(roundMultiplier);
        }
        
        // EnemyController sẽ tự động lấy tốc độ từ EnemyStats thông qua EnemyHealth.SetStats
    }
    
    private IEnumerator WaitForWaveComplete()
    {
        // Chờ cho đến khi tất cả enemy trong wave đều bị tiêu diệt hoặc đến đích
        while (activeEnemiesInWave.Count > 0)
        {
            // Xóa các enemy không còn active
            activeEnemiesInWave.RemoveAll(e => e == null || !e.activeInHierarchy);
            
            if (activeEnemiesInWave.Count == 0)
                break;
                
            yield return new WaitForSeconds(0.5f); // Kiểm tra mỗi 0.5 giây
        }
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnValidate()
    {
        // Đảm bảo luôn có spawnPoint
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
    
    #endregion
}