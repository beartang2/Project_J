using UnityEngine;

public class CheckHoveringPlayer : CheckHandTransform
{
    private float hoverTimer = 0f;
    private float hoverThreshold = 1.0f;

    public GameObject hoveringKeyPrefab_1P; // 서버 플레이어용
    public GameObject hoveringKeyPrefab_2P; // 클라이언트 플레이어용

    private GameObject assignedKey;
    private HoveringManager manager;

    public override void OnNetworkSpawn()
    {
        //if (!IsOwner) return;

        // 자동 할당
        if (OwnerClientId == 0)
        {
            assignedKey = hoveringKeyPrefab_1P;
        }
        else
        {
            Debug.Log("클라이언트 키 할당");
            assignedKey = hoveringKeyPrefab_2P;
        }

        manager = FindObjectOfType<HoveringManager>();
    }

    private void Update()
    {
        if (assignedKey == null || leftHand == null || rightHand == null)
        {
            return;
        }

        float distL = Vector3.Distance(leftHand.position, assignedKey.transform.position);
        float distR = Vector3.Distance(rightHand.position, assignedKey.transform.position);

        if (distL < 0.5f || distR < 0.5f)
        {
            hoverTimer += Time.deltaTime;

            Debug.Log($"Hovering: {OwnerClientId}"); // 디버그 로그 추가

            if (hoverTimer >= hoverThreshold)
            {
                manager.ReportHoverComplete(OwnerClientId);
            }
        }
        else
        {
            hoverTimer = 0f;
        }
    }
}
