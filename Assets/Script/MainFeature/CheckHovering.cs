using UnityEngine;

public class CheckHoveringPlayer : CheckHandTransform
{
    private float hoverTimer = 0f;
    private float hoverThreshold = 2.0f;

    [SerializeField] private GameObject assignedKey;
    private HoveringManager manager;

    public override void OnNetworkSpawn()
    {
        //if (!IsOwner) return;

        // 자동 할당
        if (OwnerClientId == 0)
        {
            assignedKey = GameObject.Find("HoveringHandKey_P1");
        }
        else
        {
            Debug.Log("클라이언트 키 할당");
            assignedKey = GameObject.Find("HoveringHandKey_P2");
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

        if (distL < 0.2f || distR < 0.2f)
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
