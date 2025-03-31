using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MergeObjects : MonoBehaviour
{
    [SerializeField] XRControllerInput xr_input;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [SerializeField] private GameObject[] keyObjects; // 키 오브젝트
    public GameObject mergedPrefab; // 병합될 새로운 프리팹
    private bool isMerged = false;

    private Vector3 betweenObjectPos;

    private void Start()
    {
        isMerged = false;
    }

    private void Update()
    {
        // 왼손 오른손 거리 계산
        float distance = Vector3.Distance(leftHand.position, rightHand.position);
        // 오브젝트 사이 위치 계산
        betweenObjectPos = GetSpawnPosition(keyObjects[0], keyObjects[1]);

        // 거리가 가까우면 & 오른쪽, 왼쪽 트리거가 동시에 눌린 상태일 때
        if (distance < 0.15f && (xr_input.isLPressed && xr_input.isRPressed))
        {
            if(!isMerged)
            {
                Instantiate(mergedPrefab, betweenObjectPos, Quaternion.identity);
                isMerged = true;

                Debug.Log("트리거 동시에 눌림");

                for(int i=0; i<keyObjects.Length; i++)
                {
                    keyObjects[i].SetActive(false);
                }
            }
        }
    }

    Vector3 GetSpawnPosition(GameObject obj1, GameObject obj2)
    {
        return (obj1.transform.position + obj2.transform.position) / 2f;
    }
}
