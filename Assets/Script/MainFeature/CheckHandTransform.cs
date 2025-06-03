using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CheckHandTransform : NetworkBehaviour
{
    public XRControllerInput xr_input;
    public Transform leftHand;
    public Transform rightHand;

    public virtual void RequestSpawnMergedObject(Vector3 spawnPos, NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
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

        if (!obj.activeSelf || !obj2.activeSelf) return false;

        if (xr_input.isLPressed && xr_input.isRPressed && distance < 1.0f && handDis < 0.15f)
        {
            Debug.Log("충분히 가까움");

            var obj1Net = obj.GetComponent<NetworkObject>();
            var obj2Net = obj2.GetComponent<NetworkObject>();

            if (IsServer)
            {
                obj.SetActive(false);
                obj2.SetActive(false);

                GameObject spawned = Instantiate(newObj, spawnPos, Quaternion.identity);
                var spawnedNet = spawned.GetComponent<NetworkObject>();
                spawnedNet.Spawn();

                DisableMergedObjectsClientRpc(
                    new NetworkObjectReference(obj1Net),
                    new NetworkObjectReference(obj2Net)
                );
            }
            else
            {
                DisableMergedObjectsServerRpc(
                    new NetworkObjectReference(obj1Net),
                    new NetworkObjectReference(obj2Net)
                );

                RequestSpawnMergedObject(
                    spawnPos,
                    new NetworkObjectReference(obj1Net),
                    new NetworkObjectReference(obj2Net)
                );
            }

            SetTeleporterCanPort(IsServer ? "Teleporter_A" : "Teleporter_B", true);
            return true;
        }

        return false;
    }

    public Vector3 GetSpawnPosition(GameObject obj1, GameObject obj2)
    {
        return (obj1.transform.position + obj2.transform.position) / 2f;
    }

    // 텔레포트 활성화
    public void SetTeleporterCanPort(string tag, bool value)
    {
        foreach (GameObject tp in GameObject.FindGameObjectsWithTag(tag))
        {
            if (tp.TryGetComponent<Teleporter>(out var teleporter))
            {
                teleporter.canPort = value;
            }
        }
    }

    [ClientRpc]
    public void DisableMergedObjectsClientRpc(NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        if (!obj1Ref.TryGet(out NetworkObject obj1))
            Debug.LogWarning("obj1Ref 참조 실패");
        else
        {
            obj1.gameObject.SetActive(false);
        }

        if (!obj2Ref.TryGet(out NetworkObject obj2))
            Debug.LogWarning("obj2Ref 참조 실패");
        else
        {
            obj2.gameObject.SetActive(false);
        }
    }

    [ServerRpc]
    public void DisableMergedObjectsServerRpc(NetworkObjectReference obj1Ref, NetworkObjectReference obj2Ref)
    {
        if (!obj1Ref.TryGet(out NetworkObject obj1))
            Debug.LogWarning("obj1Ref 참조 실패");
        else
        {
            obj1.gameObject.SetActive(false);
        }

        if (!obj2Ref.TryGet(out NetworkObject obj2))
            Debug.LogWarning("obj2Ref 참조 실패");
        else
        {
            obj2.gameObject.SetActive(false);
        }
    }
}