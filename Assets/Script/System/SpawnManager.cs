using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("Door Key")]
    [SerializeField] private GameObject[] doorKeyPrefabs;
    [SerializeField] private Transform[] doorKeySpawnPoints;

    [Header("Merge Key")]
    [SerializeField] private GameObject[] mergeKeyPrefabs;
    [SerializeField] private Transform[] mergeKeySpawnPoints;

    [Header("Jar Key")]
    [SerializeField] private GameObject[] jarKeyPrefabs;
    [SerializeField] private Transform[] jarKeySpawnPoints;

    private List<NetworkObject> spawnedObjects = new List<NetworkObject>();
    private List<NetworkObject> jarKeySpawned = new List<NetworkObject>();

    [SerializeField] private GameObject jarObj; // 항아리
    private InsertToJar jarSc; // 항아리 스크립트

    [SerializeField] private GameObject[] collisionSound; // 충돌 사운드 스크립트
    private List<CollisionSound> collisionAudioSources = new List<CollisionSound>();

    private void Awake()
    {
        jarSc = jarObj.GetComponent<InsertToJar>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnAll();
        foreach (var sound in collisionSound)
        {
            var audioSource = sound.GetComponent<CollisionSound>();
            if (audioSource != null)
            {
                collisionAudioSources.Add(audioSource);
            }
        }

        foreach(var source in collisionAudioSources)
        {
            StartCoroutine(source.muteDuringSpawn());
        }
    }

    // 전체 스폰
    public void SpawnAll()
    {
        DespawnAll();
        SpawnObjects(mergeKeyPrefabs, mergeKeySpawnPoints);
        SpawnObjects(doorKeyPrefabs, doorKeySpawnPoints);
        SpawnJarKeys(); // jar 따로 관리
    }

    // 항아리 키 전용 스폰
    private void SpawnJarKeys()
    {
        for (int i = 0; i < jarKeyPrefabs.Length && i < jarKeySpawnPoints.Length; i++)
        {
            var obj = Instantiate(jarKeyPrefabs[i], jarKeySpawnPoints[i].position, jarKeySpawnPoints[i].rotation);
            var netObj = obj.GetComponent<NetworkObject>();
            if(netObj != null)
            {
                netObj.Spawn(true);
            }
            jarKeySpawned.Add(netObj);

            Debug.Log($"[JarKey] 스폰됨: {obj.name}");
        }
    }

    // 일반 키 스폰
    private void SpawnObjects(GameObject[] prefabs, Transform[] spawnPoints)
    {
        for (int i = 0; i < prefabs.Length && i < spawnPoints.Length; i++)
        {
            var obj = Instantiate(prefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
            var netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            spawnedObjects.Add(netObj);

            Debug.Log($"[Key] 스폰됨: {obj.name}");
        }
    }

    // 전체 디스폰
    private void DespawnAll()
    {
        foreach (var netObj in spawnedObjects)
        {
            if (netObj != null && netObj.IsSpawned && !netObj.CompareTag("Player"))
            {
                netObj.Despawn(true);
            }
        }
        spawnedObjects.Clear();

        foreach (var netObj in jarKeySpawned)
        {
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
        }
        jarKeySpawned.Clear();
    }

    // 전체 리셋
    public void ResetAll()
    {
        Debug.Log("[SpawnManager] 전체 리셋");
        SpawnAll();
    }

    // 항아리 키만 리셋
    public void ResetJarKeys()
    {
        jarSc.ResetTrigger(); // 항아리 trigger 초기화

        Debug.Log("[SpawnManager] 항아리 키만 리셋");

        foreach (var netObj in jarKeySpawned)
        {
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
        }
        jarKeySpawned.Clear();

        SpawnJarKeys();
    }
}
