using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Net_PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform lobbyPos;  // 로비 위치
    [SerializeField] private Transform p1StartPos;  // P1 위치
    [SerializeField] private Transform p2StartPos;  // P2 위치

    public override void OnNetworkSpawn()
    {
        // 처음에 로비로 이동
        if (OwnerClientId == 0)
        {
            // 서버 플레이어는 로비 위치
            gameObject.transform.position = lobbyPos.position;
            gameObject.name = "Player1";
            Debug.Log("플레이어1 위치: " + transform.position);
        }
        else
        {
            // 클라이언트 플레이어는 P2 위치
            gameObject.transform.position = lobbyPos.position;
            gameObject.name = "Player2";
            Debug.Log("플레이어2 위치: " + transform.position);
        }
    }

    // P1 버튼을 누르면 실행될 함수
    public void Player1Move()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == 0) // 서버 플레이어
            {
                player.transform.position = p1StartPos.position;
                Debug.Log($"플레이어1({player.GetComponent<NetworkObject>().OwnerClientId})가 P1 위치로 이동: {player.transform.position}");
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
            }
        }
    }
}
