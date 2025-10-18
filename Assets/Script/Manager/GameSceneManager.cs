using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    // Singleton
    public static GameSceneManager Instance { get; private set; }
    private int selectedMapIndex = 1;
    
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

    public void PlayMap(int mapIndex)
    {
        selectedMapIndex = mapIndex;
        
        Debug.Log($"Loading InGame scene with Map {mapIndex}...");
        
        SceneManager.LoadScene("InGamePlay");
        
        SceneManager.sceneLoaded += OnInGameSceneLoaded;
    }

    void OnInGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "InGamePlay")
        {
            SceneManager.sceneLoaded -= OnInGameSceneLoaded;
            
            SpawnSelectedMap();
        }
    }

    void SpawnSelectedMap()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SpawnMap(selectedMapIndex);
        }
        else
        {
            Debug.LogError("MapManager not found! Đảm bảo MapManager có DontDestroyOnLoad.");
        }
    }

    public void BackToMenu()
    {
        Debug.Log("Loading Menu scene...");
        
        // Destroy map trước khi về menu
        if (MapManager.Instance != null)
        {
            MapManager.Instance.DestroyCurrentMap();
        }
        
        SceneManager.LoadScene("MapRoadScene");
    }

    public void RestartMap()
    {
        Debug.Log("Restarting map...");

        if (MapManager.Instance != null)
        {
            MapManager.Instance.SpawnMap(selectedMapIndex);
        }
    }
    public void GotoHomePage()
    {
        Debug.Log("Returning to HomePage...");

        if (MapManager.Instance != null)
        {
            MapManager.Instance.DestroyCurrentMap();
        }

        SceneManager.LoadScene("HomePage");
    }
    public void LoadShopScene()
    {
        Debug.Log("Loading Shop scene...");

        if (MapManager.Instance != null)
        {
            MapManager.Instance.DestroyCurrentMap();
        }

        SceneManager.LoadScene("ShopScene");
    }
    public void LoadMapScene()
    {
        Debug.Log("Loading RoadMap scene...");

        if (MapManager.Instance != null)
        {
            MapManager.Instance.DestroyCurrentMap();
        }

        SceneManager.LoadScene("MapRoadScene");
    }

    public int GetSelectedMapIndex()
    {
        return selectedMapIndex;
    }
    
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public bool IsInGameScene()
    {
        return GetCurrentSceneName() == "InGamePlay";
    }
}
