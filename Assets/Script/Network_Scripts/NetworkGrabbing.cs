using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkGrabbing : NetworkBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // XR 이벤트 연결
        grabInteractable.selectEntered.AddListener(OnGrab);
        //grabInteractable.selectExited.AddListener(OnRelease);
    }

    // 오브젝트 잡았을 때 (필요 시 사용)
    /*public void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("오브젝트 잡음");
    }*/

    // 오브젝트 놓았을 때 → 서버에 위치 전달
    /*public void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log("오브젝트 놓음, 서버에 위치 전달");

        if (!IsServer)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            // 서버에 위치 갱신 요청
            RequestMoveKeyServerRpc(pos, rot);
        }
    }*/

    // 서버에서 위치 적용
    /*[ServerRpc(RequireOwnership = false)]
    public void RequestMoveKeyServerRpc(Vector3 newPos, Quaternion newRot)
    {
        transform.SetPositionAndRotation(newPos, newRot);
    }*/

    public void OnGrab(SelectEnterEventArgs args)
    {
        NetworkObject netObj = GetComponent<NetworkObject>();

        if (netObj == null)
            return;

        // 현재 내가 서버라면 직접 가져오기
        if (IsServer)
        {
            if (!netObj.IsOwnedByServer)
            {
                netObj.ChangeOwnership(NetworkManager.ServerClientId);
                Debug.Log("서버가 소유권 다시 가져옴");
            }
        }
        // 클라이언트라면 요청 보내기
        else if (!IsOwner)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }

        Debug.Log("오브젝트 잡음");
    }


    [ServerRpc(RequireOwnership = false)]
    void RequestOwnershipServerRpc(ulong clientId)
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.OwnerClientId != clientId)
        {
            netObj.ChangeOwnership(clientId);
            Debug.Log($"소유권을 클라이언트 {clientId}에게 넘김");
        }
    }

}
