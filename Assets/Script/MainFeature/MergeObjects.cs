using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MergeObjects : CheckHandTransform
{
    private List<GameObject> keyObjectsA = new List<GameObject>(); // A키 오브젝트 리스트
    private List<GameObject> keyObjectsB = new List<GameObject>(); // B키 오브젝트 리스트
    private GameObject[] allKeyObjects;
    public GameObject mergedPrefab; // 병합될 새로운 프리팹

    private Vector3 betweenObjectPos;

    private void Update()
    {
        MergeObject();
    }

    public override void OnNetworkSpawn()
    {
        FindObjects();
    }
    private void FindObjects()
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
        // 오브젝트 쌍 찾기
        foreach (GameObject objA in keyObjectsA)
        {
            foreach (GameObject objB in keyObjectsB)
            {
                // 두 오브젝트가 존재하고 활성화 상태인지 확인
                if (objA.activeSelf && objB.activeSelf)
                {
                    CheckDistanceNCreate(objA, objB, mergedPrefab);
                }
            }
        }
    }

    public override void RequestSpawnMergedObject(Vector3 spawnPos)
    {
        if (IsOwner && !IsServer)
        {
            SpawnMergedObjectServerRpc(spawnPos);
        }
        else if (IsServer)
        {
            // 서버 자체에서 호출한 경우
            SpawnMergedObjectServerRpc(spawnPos);
        }
    }

    [ServerRpc]
    void SpawnMergedObjectServerRpc(Vector3 spawnPos, ServerRpcParams rpcParams = default)
    {
        GameObject newObject = Instantiate(mergedPrefab, spawnPos, Quaternion.identity);
        newObject.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        newObject.tag = "merged_key";
        newObject.name = mergedPrefab.name;

        Debug.Log("새로운 오브젝트 생성: " + newObject.name);
    }
}
