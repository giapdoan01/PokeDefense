using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour
{
    [Header("Map Info")]
    public int mapIndex = 1;
    
    [Header("UI References")]
    public TMP_Text mapText;
    
    public Image mapIcon;
    
    [Header("Visual Settings")]
    public Color hoverColor = new Color(0.8f, 0.8f, 1f);
    
    private Button button;
    private Image buttonImage;
    private Color originalColor;
    
    
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        
        UpdateMapText();
        
        ValidateMap();
    }

    void UpdateMapText()
    {
        if (mapText != null)
        {
            mapText.text = $"Map {mapIndex}";
        }
    }

    void ValidateMap()
    {
        if (MapManager.Instance != null)
        {
            MapData mapData = MapManager.Instance.GetMapByIndex(mapIndex);
            
            if (mapData == null)
            {
                Debug.LogWarning($"Map {mapIndex} không tồn tại trong MapManager!");
                
                if (button != null)
                {
                    button.interactable = false;
                }
                
                if (mapText != null)
                {
                    mapText.text = $"Map {mapIndex}\n(Locked)";
                }
            }
            else if (mapData.mapPrefab == null)
            {
                Debug.LogWarning($"Map {mapIndex} chưa có prefab!");
                
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }
    }

    void OnButtonClick()
    {
        Debug.Log($"Map {mapIndex} button clicked!");
        
        if (GameSceneManager.Instance == null)
        {
            Debug.LogError("GameSceneManager not found!");
            return;
        }
        
        GameSceneManager.Instance.PlayMap(mapIndex);
    }

    public void OnPointerEnter()
    {
        if (buttonImage != null && button != null && button.interactable)
        {
            buttonImage.color = hoverColor;
        }
    }

    public void OnPointerExit()
    {
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }
}
