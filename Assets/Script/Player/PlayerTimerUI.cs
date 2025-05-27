using TMPro;
using UnityEngine;
using Unity.Netcode;
public class PlayerTimerUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    public bool isRunning = false;
    public float timerLimit = 7200f;
    private float timer = 0f;

    private void Awake()
    {
        isRunning = false;
    }

    public void StartTimer()
    {
        if (isRunning) return; // 이미 실행 중이면 무시

        Debug.Log("타이머 시작!");
        isRunning = true;
        timer = timerLimit;
    }

    public void ResetTimer()
    {
        isRunning = false;
        timer = timerLimit;
        timerText.text = "00:00";
    }

    public bool IsFinished() => timer <= 0f;

    private void Update()
    {
        if (!isRunning) return; // isRunning이 false면 무조건 멈춤

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            isRunning = false; // 타이머 종료시 자동 정지
        }

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        if (timerText != null)
        {
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            Debug.LogWarning("timerText가 연결되지 않았습니다!");
        }
    }
}
