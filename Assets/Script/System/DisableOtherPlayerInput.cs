using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DisableOtherPlayerInput : NetworkBehaviour
{
    [SerializeField] private GameObject playerHead; // 플레이어 머리
    [SerializeField] private GameObject playerHeadObj; // 플레이어 머리 오브젝트

    private void CheckAndDisableIfNotOwner()
    {
        if (IsSpawned && !IsOwner)
        {
            DisableXRForOtherPlayer();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CheckAndDisableIfNotOwner();
        SetupCameraCullingMask();
        UpdateHeadLayer();
    }

    private void Start()
    {
        CheckAndDisableIfNotOwner();
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

        PlayerMovement playerMovement = gameObject.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // XR Controller 비활성화 (좌우 손 각각 찾아서 비활성화)
        var deviceControllers = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.XRController>();
        foreach (var controller in deviceControllers)
        {
            controller.enableInputActions = false; // 이게 없어도 무방하지만 있으면 안전
            controller.enabled = false;
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

        // TrackedPoseDriver 전부 비활성화
        var trackedPoseDrivers = GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
        foreach (var driver in trackedPoseDrivers)
        {
            driver.enabled = false;
        }
    }

    private void SetupCameraCullingMask()
    {
        if (!IsOwner) return;

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;

        int layer1PHead = LayerMask.NameToLayer("1PHead");
        int layer2PHead = LayerMask.NameToLayer("2PHead");

        if (OwnerClientId == 0)
        {
            // 클라이언트 0은 1PHead는 보이고, 2PHead는 숨김
            cam.cullingMask |= (1 << layer2PHead);
            cam.cullingMask &= ~(1 << layer1PHead);
        }
        else if (OwnerClientId == 1)
        {
            // 클라이언트 1은 2PHead는 보이고, 1PHead는 숨김
            cam.cullingMask |= (1 << layer1PHead);
            cam.cullingMask &= ~(1 << layer2PHead);
        }
    }

    private void UpdateHeadLayer()
    {
        if (playerHead != null)
        {
            if (OwnerClientId == 1)
            {
                playerHead.layer = LayerMask.NameToLayer("2PHead");
                playerHeadObj.layer = LayerMask.NameToLayer("2PHead");
                playerHeadObj.name = "Moon";
            }
            else
            {
                playerHead.layer = LayerMask.NameToLayer("1PHead");
                playerHeadObj.layer = LayerMask.NameToLayer("1PHead");
            }
        }
        else
        {
            Debug.LogWarning("Player head object is not assigned.");
        }
    }
}
