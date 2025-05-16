using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MergeObjects : CheckHandTransform, IResettable
{
    private List<GameObject> keyObjectsA = new List<GameObject>(); // A키 오브젝트 리스트
    private List<GameObject> keyObjectsB = new List<GameObject>(); // B키 오브젝트 리스트
    private GameObject[] allKeyObjects;
    public GameObject mergedPrefab; // 병합될 새로운 프리팹

    private Vector3 betweenObjectPos;
    public bool isMerged = false; // 병합 완료 플래그

    private void Update()
    {
        if (xr_input.isLPressed && xr_input.isRPressed)
        {
            MergeObject();
        }
    }

    public override void OnNetworkSpawn()
    {
        FindObjects();
    }

    public void FindObjects()
    {
        // 씬에서 모든 A/B 키 오브젝트 찾기
        allKeyObjects = GameObject.FindGameObjectsWithTag("keyObjects");

        foreach (GameObject obj in allKeyObjects)
        {
            Debug.Log(obj);
            if (obj.name.Contains("Merge_Key_A")) // A 오브젝트 찾기 (이름으로 구분)
                keyObjectsA.Add(obj);
            else if (obj.name.Contains("Merge_Key_B")) // B 오브젝트 찾기
                keyObjectsB.Add(obj);
        }
    }

    private void MergeObject()
    {
        if (isMerged) return; // 병합이 이미 되었으면 더 이상 시도 안 함

        // 오브젝트 쌍 찾기
        foreach (GameObject objA in keyObjectsA)
        {
            foreach (GameObject objB in keyObjectsB)
            {
                // 두 오브젝트가 존재하고 활성화 상태인지 확인
                if (objA.activeSelf && objB.activeSelf)
                {
                    if (CheckDistanceNCreate(objA, objB, mergedPrefab))
                    {
                        isMerged = true; // 병합 완료
                        return;
                    }
                }
            }
        }
    }

    public override void RequestSpawnMergedObject(Vector3 spawnPos)
    {
        Debug.Log($"[Client] 병합 요청: {spawnPos}");
        RequestSpawnMergedObjectServerRpc(spawnPos);
    }

    public void ResetTrigger()
    {
        isMerged = false; // 병합 완료 플래그 초기화

        Debug.Log("[MergeObjects] 트리거 상태 초기화 완료");
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestSpawnMergedObjectServerRpc(Vector3 spawnPos, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] 병합 실행: {spawnPos}");

        GameObject newObject = Instantiate(mergedPrefab, spawnPos, Quaternion.identity);
        
        var netObj = newObject.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(); // 소유권 부여하지 않음
        }

        newObject.tag = "merged_key";
        newObject.name = mergedPrefab.name;
    }
}
