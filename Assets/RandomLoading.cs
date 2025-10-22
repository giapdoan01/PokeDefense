using UnityEngine;
using UnityEngine.UI;

public class RandomLoading : MonoBehaviour
{
    public GameObject[] loadingImage;
    void Start()
    {
        //Tắt toàn bộ ảnh loading
        for (int i = 0; i < loadingImage.Length; i++)
        {
            loadingImage[i].SetActive(false);
        }
        RamdomImage();
    }
    void RamdomImage()
    {
        int rand = Random.Range(0, loadingImage.Length);
        for (int i = 0; i < loadingImage.Length; i++)
        {
            if (i == rand)
            {
                loadingImage[i].SetActive(true);
            }
            else
            {
                loadingImage[i].SetActive(false);
            }
        }
    }
}
