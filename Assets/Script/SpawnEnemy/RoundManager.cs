using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }
    public List<RoundConfig> rounds;     // Danh sách round
    public WaveSpawner spawner;
    public float delayBetweenWaves = 3f; // Thời gian chờ giữa các wave trong 1 round

    [Header("Next Round Button")]
    public Button nextRoundButton;       // Button để chuyển round
    private int currentRound = 0;
    private bool waitingForNextRound = false;
    private List<Coroutine> activeWaveCoroutines = new List<Coroutine>();

    private void Start()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.AddListener(OnNextRoundButtonClicked);
            nextRoundButton.gameObject.SetActive(false); // Ẩn button lúc đầu
        }

        StartCoroutine(PlayRounds());
    }

    // Phương thức được gọi khi nhấn nút Next Round
    public void OnNextRoundButtonClicked()
    {
        waitingForNextRound = false;
        nextRoundButton.gameObject.SetActive(false);
    }

    private IEnumerator PlayRounds()
    {
        // Tự động thêm coin của round đầu tiên khi bắt đầu game
        if (rounds.Count > 0)
        {
            PlayerStats.Instance.AddCoin(rounds[0].coinRound);
        }

        for (currentRound = 0; currentRound < rounds.Count; currentRound++)
        {
            // Chạy round hiện tại và đợi nó hoàn thành
            yield return StartCoroutine(PlayRound(currentRound));
            
            // Đợi thời gian delayBetweenWaves sau khi quái cuối cùng của round được spawn ra
            yield return new WaitForSeconds(delayBetweenWaves);
            
            // Nếu không phải là round cuối, hiện nút Next Round
            if (currentRound < rounds.Count - 1)
            {
                waitingForNextRound = true;
                nextRoundButton.gameObject.SetActive(true);
                
                // Đợi người chơi nhấn nút Next Round
                yield return new WaitUntil(() => !waitingForNextRound);
                
                // Thêm coin cho round tiếp theo
                PlayerStats.Instance.AddCoin(rounds[currentRound + 1].coinRound);
            }
        }

        // Sau khi hoàn thành tất cả các round, chờ cho đến khi không còn enemy nào
        yield return new WaitUntil(() => FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length == 0);
        
        // Gọi OnGameWin khi tất cả enemy đã bị tiêu diệt
        if (GameBattleManager.Instance != null)
        {
            GameBattleManager.Instance.OnGameWin();
        }
    }
    
    private IEnumerator PlayRound(int roundIndex)
    {
        RoundConfig roundConfig = rounds[roundIndex];
        int wavesInRound = roundConfig.waves.Count;
        activeWaveCoroutines.Clear();
        
        for (int waveIndex = 0; waveIndex < wavesInRound; waveIndex++)
        {
            WaveConfig wave = roundConfig.waves[waveIndex];
            
            // Bắt đầu spawn wave và đợi cho đến khi TẤT CẢ quái đã được spawn xong
            yield return StartCoroutine(SpawnWave(wave, roundIndex + 1));
            
            // Nếu không phải wave cuối, đợi theo waveDelay trước khi bắt đầu wave tiếp theo
            if (waveIndex < wavesInRound - 1)
            {
                // Sử dụng waveDelay của wave hiện tại
                if (wave.waveDelay > 0)
                {
                    yield return new WaitForSeconds(wave.waveDelay);
                }
            }
        }
    }
    
    // Spawn một wave và đợi cho đến khi TẤT CẢ quái đã được spawn xong
    private IEnumerator SpawnWave(WaveConfig wave, int roundMultiplier)
    {
        // Bắt đầu spawn quái
        Coroutine waveCoroutine = StartCoroutine(spawner.SpawnWave(wave, roundMultiplier, false));
        activeWaveCoroutines.Add(waveCoroutine);
        
        // Tính toán thời gian để spawn toàn bộ quái trong wave
        float totalSpawnTime = EstimateWaveSpawnTime(wave);
        
        // Đợi cho đến khi tất cả quái đã được spawn
        yield return new WaitForSeconds(totalSpawnTime);
    }
    
    // Ước tính thời gian để spawn hết quái trong wave
    private float EstimateWaveSpawnTime(WaveConfig wave)
    {
        float time = 0f;
        
        foreach (var enemyStats in wave.enemies)
        {
            if (enemyStats != null && enemyStats.spawnCount > 0)
            {
                // Tính tổng thời gian spawn cho loại quái này
                // Quái đầu tiên được spawn ngay lập tức, những con tiếp theo cần thời gian
                float enemySpawnTime = 0;
                if (enemyStats.spawnCount > 1)
                {
                    enemySpawnTime = (enemyStats.spawnCount - 1) * enemyStats.timeBetweenSpawns;
                }
                
                // Cập nhật thời gian tổng nếu loại quái này cần nhiều thời gian hơn
                time = Mathf.Max(time, enemySpawnTime);
            }
        }
        
        // Thêm một khoảng buffer nhỏ để đảm bảo
        return time + 4.0f;
    }
}