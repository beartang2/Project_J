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
        connectedPlayers.Add(player);
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
            Debug.Log($"[AllPlayerConnectedManager] Player {p.OwnerClientId} ¿¬°áµÊ");
            p.GetComponent<DisableOtherPlayerInput>().InitializeAfterBothConnected();
        }
    }
}
