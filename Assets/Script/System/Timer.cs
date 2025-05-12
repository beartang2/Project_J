using Unity.Netcode; // 추가
using UnityEngine;

public class Timer : NetworkBehaviour // 반드시 NetworkBehaviour 상속
{
    [SerializeField] private Transform lobbyPosition;
    [SerializeField] private Net_PlayerSpawner playerSpawner;

    private bool isRunning = false;

    void Update()
    {
        if (!IsServer) return;

        if (playerSpawner.pMoved && !isRunning)
        {
            isRunning = true;
            NotifyPlayersStartTimer();
        }

        if (!isRunning) return;

        PlayerTimerUI[] uis = FindObjectsOfType<PlayerTimerUI>();
        if (uis.Length > 0 && uis[0].IsFinished())
        {
            isRunning = false;
            playerSpawner.MoveToLobby();
            Debug.Log("타이머 종료: 로비로 이동!");

            // 서버의 타이머도 초기화
            foreach (var ui in uis)
            {
                if (ui.IsOwner) // 서버 오브젝트만
                {
                    ui.ResetTimer();
                }
            }

            // 클라이언트들에게 타이머 리셋과 위치 이동 요청
            NotifyPlayersResetTimerClientRpc();
        }
    }

    void NotifyPlayersStartTimer()
    {
        Debug.Log("타이머 시작 알림!");
        foreach (var ui in FindObjectsOfType<PlayerTimerUI>())
            ui.StartTimer();
        playerSpawner.pMoved = false;
    }

    [ClientRpc]
    void NotifyPlayersResetTimerClientRpc()
    {
        foreach (var ui in FindObjectsOfType<PlayerTimerUI>())
        {
            if (ui.IsOwner)
            {
                ui.ResetTimer();
                ui.transform.root.position = lobbyPosition.position;
                Debug.Log("클라이언트 타이머 종료 및 로비 이동");
            }
        }
    }
}
