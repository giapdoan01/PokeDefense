using UnityEngine;

[CreateAssetMenu(fileName = "PokemonData", menuName = "Pokemon/New Pokemon")]
public class PokemonData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string pokemonName;   // Tên: Pikachu, Raichu...
    public string type;          // Hệ: Electric, Fire, Water...

    [Header("Prefab & Evolution")]
    public GameObject prefab;        // Prefab của chính Pokemon này
    public PokemonData nextEvolution; // Data của Pokemon tiến hóa tiếp theo
}