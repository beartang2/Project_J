using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        for (int i = 0; i < objectPrefabs.Length && i < spawnPoints.Length; i++)
        {
            var obj = Instantiate(objectPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }
}