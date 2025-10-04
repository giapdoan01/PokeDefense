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
            // Sử dụng vị trí cố định thay vì transform.position
            Vector3 fixedPosition = new Vector3(-2.5f, -4f, -10f); // Có thể điều chỉnh tọa độ này theo nhu cầu
            PokemonUIManager.Instance.ShowPanel(upgrade, fixedPosition);
            
            // Hoặc có thể dùng vị trí trên màn hình:
            // PokemonUIManager.Instance.ShowPanel(upgrade, new Vector3(Screen.width/2, Screen.height/2, 0));
        }
    }
}