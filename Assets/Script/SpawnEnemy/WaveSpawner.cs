using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    public IEnumerator SpawnWave(WaveConfig wave, int roundMultiplier)
    {
        foreach (var entry in wave.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                GameObject enemy = ObjectPool.Instance.GetFromPool(entry.enemyPrefab);
                enemy.transform.position = spawnPoint.position;

                // Set multiplier cho máu
                var hp = enemy.GetComponent<EnemyHealth>();
                if (hp != null) hp.ApplyRoundMultiplier(roundMultiplier);

                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }
    }
}
