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
        Instantiate(pokemonPrefab, pos, Quaternion.identity);
        isOccupied = true;
    }
}
