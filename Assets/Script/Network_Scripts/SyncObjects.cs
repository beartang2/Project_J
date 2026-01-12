using Unity.Netcode;
using UnityEngine;

public class SyncObjects : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveKeyServerRpc(Vector3 newPosition, Quaternion newRotation)
    {
        if (!IsServer) return;

        transform.SetPositionAndRotation(newPosition, newRotation);
    }
}