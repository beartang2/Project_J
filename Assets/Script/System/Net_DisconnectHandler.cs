using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Net_DisconnectHandler : MonoBehaviour
{
    public Transform playerTransform; // 이동할 플레이어
    public Transform disconnectTr; // 이동할 위치

    public string sceneToLoadOnDisconnect = "Marge0331"; // 이동할 씬 이름

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("서버와 연결이 끊어졌습니다. 로비 씬으로 돌아갑니다.");

            // NetworkObject는 사라졌지만, 씬은 다시 로드할 수 있음
            SceneManager.LoadScene(sceneToLoadOnDisconnect);
        }
    }

}
