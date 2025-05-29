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
                        RequestSpawnMergedObject(merged.transform.position); // 명확하게 호출
                    }
                }
                else if (jar.name.Contains("RookHead") && !spawnedChessman[1])
                {
                    if (CheckDistanceNCreate(jar, merged, chessman2))
                    {
                        spawnedChessman[1] = true;
                        RequestSpawnMergedObject(merged.transform.position); // 명확하게 호출
                    }
                }

                if (spawnedChessman[0] && spawnedChessman[1])
                    return;
            }
        }
    }

    public override void RequestSpawnMergedObject(Vector3 spawnPos)
    {
        RequestSpawnMergedObjectServerRpc(spawnPos);
    }

    [ServerRpc]
    private void RequestSpawnMergedObjectServerRpc(Vector3 spawnPos)
    {
        GameObject[] jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");

        foreach (GameObject jar in jarKeys)
        {
            GameObject chessman = null;

            if (jar.name.Contains("KnightHead"))
                chessman = chessman1;
            else if (jar.name.Contains("RookHead"))
                chessman = chessman2;

            if (chessman != null)
            {
                GameObject spawned = Instantiate(chessman, spawnPos, Quaternion.identity);
                spawned.GetComponent<NetworkObject>().Spawn();
                break;
            }
        }
    }
}
