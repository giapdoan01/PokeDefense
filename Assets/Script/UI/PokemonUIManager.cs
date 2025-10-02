using UnityEngine;

public class PokemonUIManager : MonoBehaviour
{
    public static PokemonUIManager Instance; // Singleton
    [SerializeField] private GameObject panel; // Gắn panel UI vào đây
    private PokemonEvolution currentPokemon;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    // Hiện panel khi click Pokémon
    public void ShowPanel(PokemonEvolution pokemon, Vector3 worldPos)
    {
        currentPokemon = pokemon;
        panel.SetActive(true);

        // Đặt UI theo vị trí Pokémon (chuyển World -> Screen)
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        panel.transform.position = screenPos;
    }

    public void HidePanel()
    {
        panel.SetActive(false);
        currentPokemon = null;
    }

    public void OnUpgradeButton()
    {
        if (currentPokemon != null)
        {
            currentPokemon.Upgrade();
            HidePanel();
        }
    }

    public void OnRemoveButton()
    {
        if (currentPokemon != null)
        {
            // Gọi RemovePokemon() trên slot để đặt lại trạng thái isOccupied = false
            if (currentPokemon.currentSlot != null)
            {
                currentPokemon.currentSlot.RemovePokemon();
            }
            
            Destroy(currentPokemon.gameObject);
            HidePanel();
        }
    }
}