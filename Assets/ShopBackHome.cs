using UnityEngine;
using UnityEngine.UI;
public class ShopBackHome : MonoBehaviour
{
    public Button BackHomeButton;
    void Start()
    {
        BackHomeButton.onClick.AddListener(BackHome);
    }
    public void BackHome()
    {
        GameSceneManager.Instance.GotoHomePage();
    }
}
