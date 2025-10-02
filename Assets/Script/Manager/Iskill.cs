using UnityEngine;

public interface ISkill
{
    void Initialize(float damage, float range, EnemyHealth target = null, Animator pokemonAnimator = null);
}