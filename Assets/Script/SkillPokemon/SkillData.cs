using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Pokemon/SkillPokemon")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public GameObject skillPrefab;

    public float baseDamage;
    public float baseCooldown;
    public float baseRange; 
}
