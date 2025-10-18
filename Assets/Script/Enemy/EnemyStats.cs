using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public float enemyHealth = 100f;
    public float enemySpeed = 3f;
    public int enemyMoney = 10;
    public GameObject enemyPrefab;
    public int spawnCount = 1;
    public float timeBetweenSpawns = 1f;
}