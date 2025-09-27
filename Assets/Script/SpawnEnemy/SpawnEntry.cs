using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject enemyPrefab;  // Prefab quái
    public int count;               // Bao nhiêu con quái loại này trong wave
    public float spawnInterval = 0.5f;     // Thời gian delay giữa các con
}
