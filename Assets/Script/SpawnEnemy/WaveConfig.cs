using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/New Wave")]
public class WaveConfig : ScriptableObject
{
    [Tooltip("Danh sách kẻ địch trong wave này")]
    public List<EnemyStats> enemies = new List<EnemyStats>();
    
    [Tooltip("Thời gian chờ trước khi bắt đầu wave tiếp theo (giây)")]
    public float waveDelay = 5f;
}