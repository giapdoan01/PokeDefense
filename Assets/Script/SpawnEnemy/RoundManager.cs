using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI; // Import cho Button UI

public class RoundManager : MonoBehaviour
{
    public List<RoundConfig> rounds;     // Danh sách round
    public WaveSpawner spawner;
    public float delayBetweenWaves = 2f;
    
    [Header("Next Round Button")]
    public Button nextRoundButton;       // Button để chuyển round
    private int currentRound = 0;
    private bool waitingForNextRound = false;

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
        for (currentRound = 0; currentRound < rounds.Count; currentRound++)
        {
            foreach (var wave in rounds[currentRound].waves)
            {
                yield return spawner.SpawnWave(wave, currentRound + 1);
                yield return new WaitForSeconds(delayBetweenWaves);
            }
            
            // Nếu còn round tiếp theo, hiện button và chờ người chơi nhấn
            if (currentRound < rounds.Count - 1) 
            {
                waitingForNextRound = true;
                nextRoundButton.gameObject.SetActive(true);
                
                // Chờ cho đến khi người chơi nhấn nút
                yield return new WaitUntil(() => !waitingForNextRound);
            }
        }
    }
}