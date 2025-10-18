using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data", order = 0)]
public class MapData : ScriptableObject
{
    public int mapIndex = 1;
    public GameObject mapPrefab;
    
    private void OnValidate()
    {
        if (mapIndex < 1)
            mapIndex = 1;
    }
}
