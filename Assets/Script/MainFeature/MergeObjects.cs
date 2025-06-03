using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MergeObjects : CheckHandTransform, IResettable
{
    [Header("병합 대상 프리팹")]
    public GameObject mergedPrefab;
    [SerializeField] private float resetDelay = 1f;

    private List<GameObject> keyObjectsA = new();
    private List<GameObject> keyObjectsB = new();
    private GameObject[] allKeyObjects;

    public bool isMerged = false;
    public static event Action<GameObject> OnMergeCompleted;

    private void Update()
    {
        if (xr_input.isLPressed && xr_input.isRPressed)
        {
            TryMergeObjects();
        }
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(DelayedFindObjects());
    }

    private IEnumerator DelayedFindObjects()
    {
        yield return new WaitForSeconds(0.2f);
        FindObjects();
    }

    private void FindObjects()
    {
        allKeyObjects = GameObject.FindGameObjectsWithTag("keyObjects");

        foreach (GameObject obj in allKeyObjects)
        {
            if (obj.name.Contains("Merge_Key_Ear"))
                keyObjectsA.Add(obj);
            else if (obj.name.Contains("Merge_Key_Area"))
                keyObjectsB.Add(obj);
        }
    }

    private void TryMergeObjects()
    {
        if (isMerged) return;

        foreach (GameObject objA in keyObjectsA)
        {
            foreach (GameObject objB in keyObjectsB)
            {
                if (objA.activeSelf && objB.activeSelf)
                {
                    if (CheckDistanceNCreate(objA, objB, mergedPrefab))
                    {
                        Debug.Log("[MergeObjects] 병합 성공: " + mergedPrefab.name);
                        isMerged = true;
                        OnMergeCompleted?.Invoke(mergedPrefab); // 병합 완료 이벤트

                        objA.SetActive(false);
                        objB.SetActive(false);

                        Invoke(nameof(ResetTrigger), resetDelay); // 일정 시간 후 병합 가능하게
                        return;
                    }
                }
            }
        }
    }



    public void ResetTrigger()
    {
        isMerged = false;
        //Debug.Log("[MergeObjects] 병합 플래그 초기화 완료");
    }

    public override void RequestSpawnMergedObject(Vector3 spawnPos, NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        RequestSpawnMergedObjectServerRpc(spawnPos, obj1Ref, obj2Ref);
    }


    [ServerRpc]
    private void RequestSpawnMergedObjectServerRpc(Vector3 spawnPos, NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        GameObject newObject = Instantiate(mergedPrefab, spawnPos, Quaternion.identity);
        NetworkObject netObj = newObject.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

        newObject.tag = "merged_key";
        newObject.name = mergedPrefab.name;

        DisableMergedObjectsClientRpc(obj1Ref, obj2Ref);
    }
}
