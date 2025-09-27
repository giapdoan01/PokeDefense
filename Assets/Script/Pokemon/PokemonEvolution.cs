using UnityEngine;

public class PokemonEvolution : MonoBehaviour
{
    [SerializeField] private PokemonData pokemonData;
    public PokemonData Data => pokemonData;

    [HideInInspector] public PlacementSlot currentSlot; // gán khi spawn

    public void Upgrade()
    {
        if (pokemonData != null && pokemonData.nextEvolution != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            PlacementSlot slot = currentSlot;

            Destroy(gameObject);

            GameObject newPokemon = Instantiate(pokemonData.nextEvolution.prefab, pos, rot);
            PokemonEvolution newUpgrade = newPokemon.GetComponent<PokemonEvolution>();
            if (newUpgrade != null) newUpgrade.currentSlot = slot;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} không có tiến hóa tiếp theo!");
        }
    }

    public void Remove()
    {
        if (currentSlot != null)
        {
            currentSlot.RemovePokemon(); // reset slot
        }
        Destroy(gameObject);
    }
}
