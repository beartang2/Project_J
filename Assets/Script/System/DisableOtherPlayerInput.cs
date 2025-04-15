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

        TrackedPoseDriver trackDriver = GetComponentInChildren<TrackedPoseDriver>();
        if (trackDriver != null)
        {
            trackDriver.enabled = false;
        }

        // XR Ray Interactors 비활성화 (선택/터치 등 Ray 기반 인터랙션 방지)
        var rayInteractors = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.XRRayInteractor>();
        foreach (var ray in rayInteractors)
        {
            ray.enabled = false;
        }

        // XR Direct Interactors 비활성화 (손으로 직접 집는 상호작용 방지)
        var directInteractors = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.XRDirectInteractor>();
        foreach (var direct in directInteractors)
        {
            direct.enabled = false;
        }

        // Locomotion 시스템 비활성화 (이동 관련)
        var moveProviders = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.LocomotionProvider>();
        foreach (var provider in moveProviders)
        {
            provider.enabled = false;
        }

        // Turn Provider (스냅 회전, 연속 회전 등) 비활성화
        var turnProviders = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.SnapTurnProviderBase>();
        foreach (var turn in turnProviders)
        {
            turn.enabled = false;
        }
    }
}
