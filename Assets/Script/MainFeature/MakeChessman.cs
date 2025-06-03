using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MakeChessman : CheckHandTransform
{
    [SerializeField] private GameObject chessman1; // Jar_FirstHeadKey
    [SerializeField] private GameObject chessman2; // Jar_SecondHeadKey

    private bool[] spawnedChessman = new bool[2]; // 각 체스말 생성 여부
    private List<GameObject> jarKeys = new();
    private List<GameObject> mergedKeys = new();

    private void LateUpdate()
    {
        if (xr_input.isLPressed && xr_input.isRPressed)
        {
            TryMakeChessman();
        }
    }

    private void TryMakeChessman()
    {
        //Debug.Log("체스말 생성 시도");

        jarKeys = new List<GameObject>(GameObject.FindGameObjectsWithTag("jar_keyObject"));
        mergedKeys = new List<GameObject>(GameObject.FindGameObjectsWithTag("merged_key"));
        //Debug.Log($"jarKeys: {jarKeys.Count}, mergedKeys: {mergedKeys.Count}");
        foreach (GameObject jar in new List<GameObject>(jarKeys)) // 복사본 순회 (삭제 안전)
        {
            foreach (GameObject merged in new List<GameObject>(mergedKeys))
            {
                //Debug.Log($"검사 중: jar={jar.name}, merged={merged.name}");
                if (jar.name.Contains("KnightHead") && !spawnedChessman[0])
                {
                    if (CheckDistanceNCreate(jar, merged, chessman1))
                    {
                        Debug.Log("Knight 생성됨");
                        spawnedChessman[0] = true;
                        SendSpawnRequest(jar, merged);
                    }
                }
                else if (jar.name.Contains("RookHead") && !spawnedChessman[1])
                {
                    if (CheckDistanceNCreate(jar, merged, chessman2))
                    {
                        Debug.Log("Rook 생성됨");
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
