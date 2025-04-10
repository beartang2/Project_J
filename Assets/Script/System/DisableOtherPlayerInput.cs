using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
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
        XROrigin xrOrigin = GetComponentInChildren<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.gameObject.SetActive(false);
        }

        // 이동 시스템 비활성화
        LocomotionSystem locomotionSystem = GetComponentInChildren<LocomotionSystem>();
        if (locomotionSystem != null)
        {
            locomotionSystem.enabled = false;
        }

        // 추가적으로, 양손 컨트롤러 입력도 막고 싶으면
        var leftRay = GetComponentInChildren<ActionBasedController>(true);  // true면 비활성화된 것도 포함
        if (leftRay != null) leftRay.enableInputActions = false;

        var rightRay = GetComponentInChildren<ActionBasedController>(true);
        if (rightRay != null) rightRay.enableInputActions = false;
    }
}
