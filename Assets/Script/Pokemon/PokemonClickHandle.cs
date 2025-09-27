using UnityEngine;

public class PokemonClickHandler : MonoBehaviour
{
    private PokemonEvolution upgrade; 

    private void Start()
    {
        upgrade = GetComponent<PokemonEvolution>();
    }

    private void OnMouseDown() // cần Collider mới hoạt động
    {
        if (upgrade != null)
        {
            PokemonUIManager.Instance.ShowPanel(upgrade, transform.position);
        }
    }
}
