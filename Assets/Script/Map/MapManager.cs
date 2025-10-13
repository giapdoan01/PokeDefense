using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý danh sách maps và spawn map theo index
/// </summary>
public class MapManager : MonoBehaviour
{
    // Singleton
    public static MapManager Instance { get; private set; }
    
    [Header("Map Database")]
    public List<MapData> allMaps = new List<MapData>();
    
    [Header("Spawn Settings")]
    public Transform mapContainer;
    
    [Tooltip("Vị trí spawn map")]
    public Vector3 spawnPosition = Vector3.zero;
    
    [Tooltip("Rotation spawn map")]
    public Vector3 spawnRotation = Vector3.zero;
    
    private GameObject currentMapInstance;
    private MapData currentMapData;

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Initialize();
    }
    void Initialize()
    {
        if (mapContainer == null)
        {
            GameObject container = new GameObject("=== MAP CONTAINER ===");
            mapContainer = container.transform;
            mapContainer.SetParent(transform);
        }
        
        Debug.Log($"MapManager initialized with {allMaps.Count} maps");
    }
    
    public void SpawnMap(int mapIndex)
    {
        MapData mapData = GetMapByIndex(mapIndex);
        
        if (mapData == null)
        {
            Debug.LogError($"Map index {mapIndex} không tồn tại trong danh sách!");
            return;
        }
        
        if (mapData.mapPrefab == null)
        {
            Debug.LogError($"Map index {mapIndex} chưa có prefab!");
            return;
        }
        
        DestroyCurrentMap();
        
        Debug.Log($"Spawning Map {mapIndex}...");
        
        Quaternion rotation = Quaternion.Euler(spawnRotation);
        currentMapInstance = Instantiate(
            mapData.mapPrefab,
            spawnPosition,
            rotation,
            mapContainer
        );
        
        currentMapInstance.name = $"Map_{mapIndex}";
        currentMapData = mapData;
        
        Debug.Log($"Map {mapIndex} spawned successfully!");
    }

    public void DestroyCurrentMap()
    {
        if (currentMapInstance != null)
        {
            Debug.Log($"Destroying current map...");
            Destroy(currentMapInstance);
            currentMapInstance = null;
            currentMapData = null;
        }
    }

    public MapData GetMapByIndex(int mapIndex)
    {
        return allMaps.Find(m => m.mapIndex == mapIndex);
    }

    public GameObject GetCurrentMapInstance()
    {
        return currentMapInstance;
    }

    public MapData GetCurrentMapData()
    {
        return currentMapData;
    }

    public bool HasMapSpawned()
    {
        return currentMapInstance != null;
    }

    public int GetTotalMaps()
    {
        return allMaps.Count;
    }
}
