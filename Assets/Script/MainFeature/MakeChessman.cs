using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MakeChessman : CheckHandTransform
{
    [SerializeField] private GameObject chessman1; // Jar_FirstHeadKey
    [SerializeField] private GameObject chessman2; // Jar_SecondHeadKey

    private bool[] spawnedChessman = new bool[2]; // 각 체스말 생성 여부

    private void OnEnable()
    {
        MergeObjects.OnMergeCompleted += TryMakeChessman;
    }

    private void OnDisable()
    {
        MergeObjects.OnMergeCompleted -= TryMakeChessman;
    }

    private void TryMakeChessman(GameObject mergedPrefab)
    {
        GameObject[] jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");
        GameObject[] mergedKeys = GameObject.FindGameObjectsWithTag("merged_key");

        foreach (GameObject jar in jarKeys)
        {
            for (int i = 0; i < mergedKeys.Length; i++)
            {
                GameObject merged = mergedKeys[i];

                if (!merged.activeSelf || !jar.activeSelf)
                    continue;

                if (jar.name.Contains("KnightHead") && !spawnedChessman[0])
                {
                    if (CheckDistanceNCreate(jar, merged, chessman1))
                    {
                        spawnedChessman[0] = true;
                        SendSpawnRequest(jar, merged);
                    }
                }
                else if (jar.name.Contains("RookHead") && !spawnedChessman[1])
                {
                    if (CheckDistanceNCreate(jar, merged, chessman2))
                    {
                        spawnedChessman[1] = true;
                        SendSpawnRequest(jar, merged);
                    }
                }

                if (spawnedChessman[0] && spawnedChessman[1])
                    return;
            }
        }
    }

    public override void RequestSpawnMergedObject(Vector3 spawnPos, NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        RequestSpawnMergedObjectServerRpc(spawnPos, obj1Ref, obj2Ref);
    }

    [ServerRpc]
    private void RequestSpawnMergedObjectServerRpc(Vector3 spawnPos, NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        obj1Ref.TryGet(out NetworkObject obj1);
        obj2Ref.TryGet(out NetworkObject obj2);

        GameObject[] jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");

        GameObject chessman = null;

        foreach (GameObject jar in jarKeys)
        {
            if (jar.name.Contains("KnightHead"))
            {
                chessman = chessman1;
                break;
            }
            else if (jar.name.Contains("RookHead"))
            {
                chessman = chessman2;
                break;
            }
        }

        if (chessman != null)
        {
            GameObject spawned = Instantiate(chessman, spawnPos, Quaternion.identity);
            spawned.GetComponent<NetworkObject>().Spawn();
        }

        // 재료 제거는 반드시 마지막에 호출
        DisableMergedObjectsClientRpc(obj1Ref, obj2Ref);
    }

    private void SendSpawnRequest(GameObject jar, GameObject merged)
    {
        if (jar.TryGetComponent<NetworkObject>(out var jarNet) &&
            merged.TryGetComponent<NetworkObject>(out var mergedNet))
        {
            var jarRef = new NetworkObjectReference(jarNet);
            var mergedRef = new NetworkObjectReference(mergedNet);

            RequestSpawnMergedObject(merged.transform.position, jarRef, mergedRef);
        }
        else
        {
            Debug.LogWarning("jar 또는 merged 오브젝트에 NetworkObject가 없습니다.");
        }
    }
}
