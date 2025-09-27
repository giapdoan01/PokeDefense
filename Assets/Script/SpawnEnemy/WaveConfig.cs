using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/New Wave")]
public class WaveConfig : ScriptableObject
{
    public List<SpawnEntry> enemies; // Danh sách loại quái trong wave
}
