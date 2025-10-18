using UnityEngine;

public interface ISlowEffectSource
{
    int EffectID { get; }
    
    float SlowPercent { get; }
}