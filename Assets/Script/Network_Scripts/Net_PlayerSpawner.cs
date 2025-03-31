using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Net_PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform serverStartPos;  // P1 위치
    [SerializeField] private Transform clientStartPos;  // P2 위치

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == 0)
        {
            // 서버 플레이어는 P1 위치
            gameObject.transform.position = serverStartPos.position;
            Debug.Log("플레이어1 위치: " + transform.position);
        }
        else
        {
            // 클라이언트 플레이어는 P2 위치
            gameObject.transform.position = clientStartPos.position;
            Debug.Log("플레이어2 위치: " + transform.position);
        }
    }
}
