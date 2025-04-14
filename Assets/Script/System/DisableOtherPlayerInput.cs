using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DisableOtherPlayerInput : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // 내가 소유하지 않은 오브젝트라면 XR 관련 기능 비활성화
        if (!IsOwner)
        {
            DisableXRForOtherPlayer();
        }
    }

    private void DisableXRForOtherPlayer()
    {
        // 카메라 루트 비활성화
        Camera camera = GetComponentInChildren<Camera>();
        if (camera != null)
        {
            camera.enabled = false;
        }

        // AudioListener도 같이 비활성화
        AudioListener audioListener = camera.GetComponent<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = false;
        }
    }
}
