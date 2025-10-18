using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
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

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.UnlockMap(1);
        }
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

        Quaternion rotation = Quaternion.Euler(spawnRotation);
        currentMapInstance = Instantiate(
            mapData.mapPrefab,
            spawnPosition,
            rotation,
            mapContainer
        );

        currentMapInstance.name = $"{mapIndex}";
        currentMapData = mapData;
    }

    public void DestroyCurrentMap()
    {
        if (currentMapInstance != null)
        {
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

    public void UnlockNextMap(int currentMapIndex)
    {
        // Đảm bảo map hiện tại luôn được mở khóa
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.UnlockMap(currentMapIndex);
        }

        // Mở khóa map tiếp theo
        int nextMapIndex = currentMapIndex + 1;

        MapData nextMap = GetMapByIndex(nextMapIndex);

        if (nextMap != null)
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.UnlockMap(nextMapIndex);
            }
            else
            {
                Debug.LogWarning("PlayerDataManager không tồn tại!");
            }
        }

        // Đảm bảo Map 1 luôn được mở khóa
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.UnlockMap(1);
        }
    }
}