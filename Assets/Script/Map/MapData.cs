using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data", order = 0)]
public class MapData : ScriptableObject
{
    [Header("Map Info")]
    public int mapIndex = 1;
    
    [Header("Map Prefab")]
    public GameObject mapPrefab;
    
    // Validation
    private void OnValidate()
    {
        if (mapIndex < 1)
            mapIndex = 1;
    }
}
