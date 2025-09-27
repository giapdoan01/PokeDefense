using UnityEngine;

public class PlacementSlot : MonoBehaviour
{
    public bool isOccupied = false;
    public Transform placePoint;

    public bool CanPlace() => !isOccupied;

    public void PlacePokemon(GameObject pokemonPrefab)
    {
        if (isOccupied) return;

        Vector3 pos = placePoint ? placePoint.position : transform.position;
        GameObject pokemon = Instantiate(pokemonPrefab, pos, Quaternion.identity);

        // Gán slot vào PokemonUpgrade
        PokemonEvolution upgrade = pokemon.GetComponent<PokemonEvolution>();
        if (upgrade != null)
            upgrade.currentSlot = this;

        isOccupied = true;
    }
    public void RemovePokemon()
    {
        isOccupied = false;
    }
}
