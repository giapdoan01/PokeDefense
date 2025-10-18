using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameBattleManager : MonoBehaviour
{
    public static GameBattleManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxEnemyEscapes = 10;
    private int currentEnemyEscapes = 0;

    [Header("Win/Lose Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win Panel UI")]
    [SerializeField] private TextMeshProUGUI gemRewardText;
    [SerializeField] private GameObject WinUIMakeUp;
    [SerializeField] private int gemRewardAmount = 50;

    [Header("Lose Panel UI")]
    [SerializeField] private Button backHomeButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private GameObject loseUIMakeUp;

    [Header("Scene Names")]
    [SerializeField] private string mapRoadSceneName = "MapRoadScene";

    private bool isGameOver = false;
    private int currentMapIndex = -1;
    private Animator loseAnim;
    private Animator winAnim;
    private int GemReceive;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        if (backHomeButton != null)
            backHomeButton.onClick.AddListener(BackToHome);

        if (replayButton != null)
            replayButton.onClick.AddListener(ReplayMap);

        if (loseUIMakeUp != null)
            loseAnim = loseUIMakeUp.GetComponent<Animator>();

        if (WinUIMakeUp != null)
            winAnim = WinUIMakeUp.GetComponent<Animator>();

        if (MapManager.Instance != null && MapManager.Instance.GetCurrentMapData() != null)
        {
            currentMapIndex = MapManager.Instance.GetCurrentMapData().mapIndex;
        }

    }

    public void OnEnemyEscaped()
    {
        if (isGameOver) return;

        currentEnemyEscapes++;

        if (currentEnemyEscapes >= maxEnemyEscapes)
        {
            OnGameLose();
        }
    }

    public void OnGameWin()
    {
        if (isGameOver) return;

        isGameOver = true;

        bool alreadyCompletedMap = false;
        if (PlayerDataManager.Instance != null && currentMapIndex > 0)
        {
            GemReceive = gemRewardAmount;
            alreadyCompletedMap = PlayerDataManager.Instance.IsMapCompleted(currentMapIndex);
        }

        if (!alreadyCompletedMap && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.AddGem(gemRewardAmount);
            GemReceive = 0;

            PlayerDataManager.Instance.MarkMapCompleted(currentMapIndex);
        }

        if (currentMapIndex > 0 && MapManager.Instance != null)
        {
            MapManager.Instance.UnlockNextMap(currentMapIndex);
        }

        StartCoroutine(ShowWinPanelAndReturn());
    }

    IEnumerator ShowWinPanelAndReturn()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (gemRewardText != null)
            {
                gemRewardText.text = $"+{GemReceive} Gems";
            }
        }

        Time.timeScale = 0f;
        if (winAnim != null)
        {
            winAnim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(mapRoadSceneName);
    }

    void OnGameLose()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            loseAnim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        Time.timeScale = 0f;
    }

    void BackToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mapRoadSceneName);
        Debug.Log("Back to MapRoadScene");
    }

    void ReplayMap()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Replay current map");
    }

    public void ResetGame()
    {
        currentEnemyEscapes = 0;
        isGameOver = false;
        Time.timeScale = 1f;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        int coin = RoundManager.Instance.rounds[0].coinRound;

        PlayerStats.Instance.AddCoin(coin); 

        Debug.Log("Game reset");
    }

    public int GetCurrentEscapes() => currentEnemyEscapes;
    public int GetMaxEscapes() => maxEnemyEscapes;
    public bool IsGameOver() => isGameOver;

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}