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
        if (!IsOwner)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }

        Debug.Log("오브젝트 잡음");
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestOwnershipServerRpc(ulong clientId)
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.ChangeOwnership(clientId);
        }
    }

}
