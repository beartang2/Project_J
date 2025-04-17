using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CheckHandTransform : NetworkBehaviour
{
    public XRControllerInput xr_input;
    public Transform leftHand;
    public Transform rightHand;

    public virtual void RequestSpawnMergedObject(Vector3 spawnPos)
    {
        // 자식 클래스에서 override해서 RPC 호출
        Debug.LogWarning("RequestSpawnMergedObject는 자식 클래스에서 오버라이드되어야 합니다.");
    }

    public bool CheckDistanceNCreate(GameObject obj, GameObject obj2, GameObject newObj)
    {
        float distance = Vector3.Distance(obj.transform.position, obj2.transform.position);
        Vector3 spawnPos = GetSpawnPosition(obj, obj2);
        float handDis = Vector3.Distance(leftHand.position, rightHand.position);

        if (distance < 0.15f && handDis < 0.15f && xr_input.isLPressed && xr_input.isRPressed)
        {
            if (IsServer)
            {
                RequestSpawnMergedObject(spawnPos);

                obj.GetComponent<NetworkObject>().Despawn(true);
                obj2.GetComponent<NetworkObject>().Despawn(true);
                SetTeleporterCanPortByTag("Teleporter_A", true); // P1용
            }
            else if(IsClient && !IsServer)
{
                SetTeleporterCanPortByTag("Teleporter_B", true); // P2용
            }
            

            return true; // 병합 성공
        }

        return false;
    }


    Vector3 GetSpawnPosition(GameObject obj1, GameObject obj2)
    {
        return (obj1.transform.position + obj2.transform.position) / 2f;
    }

    // 텔레포트 활성화
    public void SetTeleporterCanPortByTag(string tag, bool value)
    {
        GameObject[] teleporters = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject tp in teleporters)
        {
            Debug.Log(tp);

            tp.GetComponent<Teleporter>().canPort = value;
        }
    }
}
