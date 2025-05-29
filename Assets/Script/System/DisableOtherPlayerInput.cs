using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DisableOtherPlayerInput : NetworkBehaviour
{
    [SerializeField] private GameObject playerHead; // 플레이어 머리
    [SerializeField] private GameObject playerHeadObj; // 플레이어 머리 오브젝트
    // 카메라 오브젝트
    private Camera p2_camera; // 2p 카메라
    private GameObject player2;     // 2p 오브젝트

    private void CheckAndDisableIfNotMine()
    {
        // 내 로컬 클라이언트 ID
        ulong localId = NetworkManager.Singleton.LocalClientId;

        // 상대방 오브젝트라면 XR 비활성화
        if (OwnerClientId != localId)
        {
            Debug.Log($"[Client {localId}] 상대 오브젝트 감지 → XR 비활성화 시도 (Owner: {OwnerClientId})");
            DisableXRForOtherPlayer();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CheckAndDisableIfNotMine();
        SetupCameraCullingMask();
        UpdateHeadLayer();
        // 나중에 접속한 다른 클라이언트를 위해 콜백 등록
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        /*        if(!IsOwner)
                {
                    // 2P 카메라를 찾기
                    player2 = GameObject.FindGameObjectWithTag("Player");
                    if (player2 != null)
                    {
                        p2_camera = player2.GetComponentInChildren<Camera>();
                        if (p2_camera != null)
                        {
                            DisableXRForOtherPlayer();
                        }
                        else
                        {
                            Debug.LogWarning("2P 카메라를 찾을 수 없습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Player2 오브젝트를 찾을 수 없습니다.");
                    }
                }*/
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

        // TrackedPoseDriver 비활성화
        TrackedPoseDriver trackedPoseDriver = camera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
        if (trackedPoseDriver != null)
        {
            trackedPoseDriver.enabled = false;
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

    private void OnClientConnected(ulong clientId)
    {
        // 새로 접속한 클라이언트에게 이 오브젝트가 "상대방 오브젝트"라면 XR 끄라고 지시
        if (clientId != OwnerClientId)
        {
            DisableXRForTargetClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void DisableXRForTargetClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            Debug.Log($"[Client {targetClientId}] 늦게 접속한 클라이언트용 XR 비활성화 실행");
            DisableXRForOtherPlayer();
        }
    }

}
