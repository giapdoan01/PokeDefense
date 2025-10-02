using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Pokemon/New Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public GameObject skillPrefab;

    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseCooldown = 1f;
    public float baseRange = 5f;
}
