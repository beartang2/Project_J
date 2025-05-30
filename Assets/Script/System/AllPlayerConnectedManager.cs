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
            foreach (var p in connectedPlayers)
            {
                p.GetComponent<DisableOtherPlayerInput>().InitializeAfterBothConnected();
            }
        }
    }
}
