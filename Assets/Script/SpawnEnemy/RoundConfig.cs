using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoundConfig", menuName = "Configs/Round Config")]
public class RoundConfig : ScriptableObject
{
    public List<WaveConfig> waves; // Các wave thuộc round này
    public int coinRound; // Số coin thưởng khi hoàn thành round
}