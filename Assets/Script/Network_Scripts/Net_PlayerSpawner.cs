using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class Net_PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform lobbyPos;  // 로비 위치
    [SerializeField] private Transform p1StartPos;  // P1 위치
    [SerializeField] private Transform p2StartPos;  // P2 위치

    public bool pMoved = false;

    void Start()
    {
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public override void OnNetworkSpawn()
    {
        MoveToLobby();
    }

    public void MoveToLobby()
    {
        if (OwnerClientId == 0)
        {
            // 서버 플레이어는 로비 위치
            gameObject.transform.position = lobbyPos.position;
            gameObject.transform.rotation = lobbyPos.rotation;
            gameObject.name = "Player" + OwnerClientId;
            //Debug.Log("플레이어" + OwnerClientId + " 위치: " + transform.position);
        }
        else if (OwnerClientId > 0)
        {
            // 클라이언트 플레이어는 P2 위치 + 오프셋
            Vector3 offset = new Vector3(1.5f, 0f, 0f);
            gameObject.transform.position = lobbyPos.position + offset;
            gameObject.transform.rotation = lobbyPos.rotation;
            gameObject.name = "Player" + OwnerClientId;
            //Debug.Log("플레이어" + OwnerClientId + " 위치: " + transform.position);
        }
    }


    // 서버가 로비에서 버튼을 눌렀을 때 호출
    public void OnStartButtonPressed()
    {
        if (!IsServer) return;

        // 전체 트리거 리셋
        ResettingManager.Instance.ResetAllTriggers();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var netObj = player.GetComponent<NetworkObject>();
            if (netObj.OwnerClientId == 0)
            {
                player.transform.position = p1StartPos.position;
                player.transform.rotation = p1StartPos.rotation;
            }
            else if (netObj.OwnerClientId == 1)
            {
                // 클라이언트에게 강제로 이동 요청
                MoveClientPlayerClientRpc(p2StartPos.position, p2StartPos.rotation.eulerAngles);
            }
        }

        pMoved = true;
    }

    [ClientRpc]
    private void MoveClientPlayerClientRpc(Vector3 newPosition, Vector3 newRotationEuler)
    {
        if (IsServer) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var netObj = player.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == 1)
            {
                player.transform.position = newPosition;
                player.transform.rotation = Quaternion.Euler(newRotationEuler);

                var timer = player.GetComponent<PlayerTimerUI>();
                if (timer != null)
                    timer.StartTimer();

                break;
            }
        }
    }


    /*
    // 서버가 버튼을 누르면 실행될 함수
    public void PlayerMove()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == 0) // 서버 플레이어=host
            {
                player.transform.position = p1StartPos.position;
                Debug.Log($"플레이어1({player.GetComponent<NetworkObject>().OwnerClientId})가 P1 위치로 이동: {player.transform.position}");
                pMoved = true;
            }
        }
    }

    // P2 버튼을 누르면 실행될 함수
    public void Player2Move()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != 0) // 클라이언트 플레이어
            {
                player.transform.position = p2StartPos.position;
                Debug.Log($"플레이어2({player.GetComponent<NetworkObject>().OwnerClientId})가 P2 위치로 이동: {player.transform.position}");
                p2Moved = true; // P2이 이동했음을 기록
            }
        }
    }
    */
}
