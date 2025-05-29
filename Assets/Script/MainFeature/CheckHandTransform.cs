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

        //Debug.Log($"거리: {distance}, 손 거리: {handDis}"); // 디버그 로그 추가

        if (distance < 1.5f && handDis < 0.15f && xr_input.isLPressed && xr_input.isRPressed)
        {
            Debug.Log("충분히 가까움");

            obj.SetActive(false);
            obj2.SetActive(false);

            if (IsServer)
            {
                // 오브젝트 스폰
                GameObject spawned = Instantiate(newObj, spawnPos, Quaternion.identity);
                spawned.GetComponent<NetworkObject>().Spawn();

                DisableMergedObjectsClientRpc(
                    obj.GetComponent<NetworkObject>(),
                    obj2.GetComponent<NetworkObject>()
                ); // 비활성화는 클라이언트에도 적용해야 함

                SetTeleporterCanPortByTag("Teleporter_A", true); // P1용
            }
            else
            {
                Debug.Log("오브젝트 스폰 요청!");
                RequestSpawnMergedObject(spawnPos);

                DisableMergedObjectsClientRpc(
                    obj.GetComponent<NetworkObject>(),
                    obj2.GetComponent<NetworkObject>()
                ); // 클라이언트에서도 비활성화

                SetTeleporterCanPortByTag("Teleporter_B", true); // P2용
            }

            return true; // 병합 성공
        }

        return false;
    }

    public Vector3 GetSpawnPosition(GameObject obj1, GameObject obj2)
    {
        return (obj1.transform.position + obj2.transform.position) / 2f;
    }

    // 텔레포트 활성화
    public void SetTeleporterCanPortByTag(string tag, bool value)
    {
        GameObject[] teleporters = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject tp in teleporters)
        {
            if (tp.GetComponent<Teleporter>() != null)
            {
                tp.GetComponent<Teleporter>().canPort = value;
            }
        }
    }

    [ClientRpc]
    void DisableMergedObjectsClientRpc(NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        if (obj1Ref.TryGet(out NetworkObject obj1))
            obj1.gameObject.SetActive(false);
        if (obj2Ref.TryGet(out NetworkObject obj2))
            obj2.gameObject.SetActive(false);
    }
}