using UnityEngine;
using UnityEngine.UI;

public class HomePageController : MonoBehaviour
{
    public Button ShopButton;
    public Button PlayButton;
    public Button BagButton;
    public GameObject BagPanel;
    public Button BackHomeButton;
    public GameObject DeckPanel;

    void Start()
    {
        if (ShopButton != null)
            ShopButton.onClick.AddListener(OnShopButtonClicked);

        if (PlayButton != null)
            PlayButton.onClick.AddListener(OnPlayButtonClicked);
        if (BagPanel != null)
            BagPanel.SetActive(false);
        if (BagButton != null)
            BagButton.onClick.AddListener(OpenBagPanel);
        if (BackHomeButton != null)
            BackHomeButton.onClick.AddListener(CloseBagPanel);
    }
    void OnShopButtonClicked()
    {
        GameSceneManager.Instance.LoadShopScene();
    }
    void OnPlayButtonClicked()
    {
        GameSceneManager.Instance.LoadMapScene();
    }
    void OpenBagPanel()
    {
        if (BagPanel != null)
            BagPanel.SetActive(true);
        if (DeckPanel != null)
            DeckPanel.SetActive(false);
    }
    void CloseBagPanel()
    {
        if (BagPanel != null)
            BagPanel.SetActive(false);
        if (DeckPanel != null)
            DeckPanel.SetActive(true);
    }
}
