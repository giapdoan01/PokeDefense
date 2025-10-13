using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI gemText;
    
    void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataLoaded += UpdateUI;
            PlayerDataManager.Instance.OnGemChanged += UpdateGem;
            
            if (PlayerDataManager.Instance.currentPlayerData != null)
            {
                UpdateUI(PlayerDataManager.Instance.currentPlayerData);
            }
        }
    }
    
    void UpdateUI(PlayerData data)
    {
        if (usernameText != null)
            usernameText.text = data.username;
        
        if (gemText != null)
            gemText.text = data.gem.ToString();
    }
    
    void UpdateGem(int gem)
    {
        if (gemText != null)
            gemText.text = gem.ToString();
    }
    
    void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataLoaded -= UpdateUI;
            PlayerDataManager.Instance.OnGemChanged -= UpdateGem;
        }
    }
}
