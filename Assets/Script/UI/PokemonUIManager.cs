using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokemonUIManager : MonoBehaviour
{
    public static PokemonUIManager Instance;
    
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    
    [Header("Pokemon Info")]
    [SerializeField] private TextMeshProUGUI pokemonNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Skill Stats")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    
    [Header("Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button evolutionButton; // ✅ NÚT EVOLUTION MỚI
    [SerializeField] private Button removeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    
    private PokemonEvolution currentPokemon;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        panel.SetActive(false);
        
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButton);
        
        // ✅ THÊM LISTENER CHO NÚT EVOLUTION
        if (evolutionButton != null)
        {
            evolutionButton.onClick.AddListener(OnEvolutionButton);
            evolutionButton.gameObject.SetActive(false); // Ẩn mặc định
        }
            
        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveButton);
    }

    public void ShowPanel(PokemonEvolution pokemon, Vector3 worldPos)
    {
        currentPokemon = pokemon;
        panel.SetActive(true);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        panel.transform.position = screenPos;
        
        UpdateUI();
    }

    public void HidePanel()
    {
        panel.SetActive(false);
        currentPokemon = null;
    }

    private void UpdateUI()
    {
        if (currentPokemon == null)
        {
            Debug.LogWarning("⚠️ currentPokemon is NULL!");
            return;
        }
        
        var skillController = currentPokemon.GetComponent<SkillController>();
        if (skillController == null)
        {
            Debug.LogWarning("⚠️ SkillController NOT FOUND on " + currentPokemon.name);
            return;
        }
        
        var skillData = skillController.CurrentSkillData;
        
        if (skillData == null)
        {
            Debug.LogWarning("⚠️ CurrentSkillData is NULL!");
            return;
        }
        
        // ✅ POKEMON INFO
        if (pokemonNameText != null)
        {
            string cleanName = currentPokemon.name.Replace("(Clone)", "").Trim();
            pokemonNameText.text = cleanName;
        }
            
        if (levelText != null)
        {
            string levelInfo = $"Level {skillController.CurrentLevel}/{skillController.MaxLevel}";
            levelText.text = levelInfo;
        }
        
        // ✅ SKILL STATS
        if (damageText != null)
        {
            damageText.text = $"Damage: {skillData.baseDamage} dmg";
        }
            
        if (rangeText != null)
        {
            rangeText.text = $"Range: {skillData.baseRange} m";
        }
            
        if (cooldownText != null)
        {
            cooldownText.text = $"Cooldown: {skillData.baseCooldown} s";
        }
        
        // ✅ BUTTON LOGIC
        bool canUpgrade = skillController.CanUpgrade;
        bool isMaxLevel = skillController.CurrentLevel >= skillController.MaxLevel;
        bool canEvolve = isMaxLevel && currentPokemon.Data != null && currentPokemon.Data.nextEvolution != null;
        
        // ✅ NÚT UPGRADE
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(!isMaxLevel); // Ẩn khi max level
            upgradeButton.interactable = canUpgrade;
        }
            
        if (upgradeButtonText != null)
        {
            upgradeButtonText.text = canUpgrade ? "UPGRADE" : "MAX LEVEL";
        }
        
        // ✅ NÚT EVOLUTION
        if (evolutionButton != null)
        {
            evolutionButton.gameObject.SetActive(canEvolve); // Chỉ hiện khi max level + có evolution
        }
    }

    public void OnUpgradeButton()
    {
        if (currentPokemon == null) return;
        
        var skillController = currentPokemon.GetComponent<SkillController>();
        if (skillController != null && skillController.CanUpgrade)
        {
            skillController.UpgradeSkill();
            UpdateUI(); // ✅ Refresh UI
        }
    }

    // ✅ HÀM XỬ LÝ EVOLUTION
    public void OnEvolutionButton()
    {
        if (currentPokemon == null) return;
        
        var skillController = currentPokemon.GetComponent<SkillController>();
        
        // Kiểm tra điều kiện evolution
        if (skillController != null && 
            skillController.CurrentLevel >= skillController.MaxLevel &&
            currentPokemon.Data != null && 
            currentPokemon.Data.nextEvolution != null)
        {
            Debug.Log($"🔥 Evolution: {currentPokemon.name} → {currentPokemon.Data.nextEvolution.pokemonName}");
            
            currentPokemon.Upgrade(); // Gọi hàm evolution có sẵn
            HidePanel(); // Đóng UI sau khi evolution
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot evolve! Conditions not met.");
        }
    }

    public void OnRemoveButton()
    {
        if (currentPokemon == null) return;
        
        if (currentPokemon.currentSlot != null)
        {
            currentPokemon.currentSlot.RemovePokemon();
        }
        
        Destroy(currentPokemon.gameObject);
        HidePanel();
    }
}
