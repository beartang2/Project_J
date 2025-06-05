using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AllPlayerConnectedManager : MonoBehaviour
{
    public static AllPlayerConnectedManager Instance;
    public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void RegisterPlayer(NetworkObject player)
    {
        Debug.Log($"[AllPlayerConnectedManager] Player {player.OwnerClientId} 연결됨");
        connectedPlayers.Add(player);
        Debug.Log($"[AllPlayerConnectedManager] 현재 연결된 플레이어 수: {connectedPlayers.Count}");
        if (connectedPlayers.Count == 2)
        {
            StartCoroutine(DelayedInitialize());
        }
    }

    private IEnumerator DelayedInitialize()
    {
        yield return new WaitForSeconds(0.05f);

        foreach (var p in connectedPlayers)
        {
            p.GetComponent<DisableOtherPlayerInput>().InitializeAfterBothConnected();
        }
    }
}
