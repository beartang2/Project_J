using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        for (int i = 0; i < objectPrefabs.Length && i < spawnPoints.Length; i++)
        {
            var obj = Instantiate(objectPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
            obj.GetComponent<NetworkObject>().Spawn();
            Debug.Log("오브젝트 소환됨");
        }
    }
}