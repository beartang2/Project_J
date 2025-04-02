using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MergeObjects : CheckHandTransform
{
    private List<GameObject> keyObjectsA = new List<GameObject>(); // A키 오브젝트 리스트
    private List<GameObject> keyObjectsB = new List<GameObject>(); // B키 오브젝트 리스트

    public GameObject mergedPrefab; // 병합될 새로운 프리팹

    private Vector3 betweenObjectPos;

    private void Start()
    {
        // 씬에서 모든 A/B 키 오브젝트 찾기
        GameObject[] allKeyObjects = GameObject.FindGameObjectsWithTag("keyObjects");

        foreach (GameObject obj in allKeyObjects)
        {
            if (obj.name.Contains("Merge_Key_A")) // A 오브젝트 찾기 (이름으로 구분)
                keyObjectsA.Add(obj);
            else if (obj.name.Contains("Merge_Key_B")) // B 오브젝트 찾기
                keyObjectsB.Add(obj);
        }
    }

    private void Update()
    {
        MergeObject();
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
}
