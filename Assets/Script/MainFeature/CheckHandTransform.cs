using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHandTransform : MonoBehaviour
{
    public XRControllerInput xr_input;
    public Transform leftHand;
    public Transform rightHand;

    public void CheckDistanceNCreate(GameObject obj, GameObject obj2, GameObject newObj)
    {
        // 오브젝트 거리 계산
        float distance = Vector3.Distance(obj.transform.position, obj2.transform.position);
        Vector3 spawnPos = GetSpawnPosition(obj, obj2);

        // 두 오브젝트가 가까워지고, 트리거 버튼이 동시에 눌린 경우
        if (distance < 0.15f && xr_input.isLPressed && xr_input.isRPressed)
        {
            GameObject newObject = Instantiate(newObj, spawnPos, Quaternion.identity);

            //if(newObject.name == "Merged_Key(Clone)")
            //{
            //    newObject.name = "Merged_Key";  // (Clone) 삭제
            //}

            obj.SetActive(false);
            obj2.SetActive(false);

            SetAllTeleportersTrue();

            Debug.Log("새로운 오브젝트 생성!");
        }
    }

    Vector3 GetSpawnPosition(GameObject obj1, GameObject obj2)
    {
        return (obj1.transform.position + obj2.transform.position) / 2f;
    }

    // 모든 Teleporter의 canPort를 true로 변경하는 함수
    public void SetAllTeleportersTrue()
    {
        Teleporter[] teleporters = FindObjectsOfType<Teleporter>(); // 모든 Teleporter 찾기
        foreach (Teleporter tele in teleporters)
        {
            tele.canPort = true;
        }
    }
}
