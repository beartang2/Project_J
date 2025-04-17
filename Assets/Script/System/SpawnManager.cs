using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        { return; }

        for (int i = 0; i < objectPrefabs.Length && i < spawnPoints.Length; i++)
        {
            var obj = Instantiate(objectPrefabs[i], spawnPoints[i].position, Quaternion.identity);
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn(true);

            Debug.Log("오브젝트 소환됨");
        }

    }
}