using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CheckHovering : CheckHandTransform
{
    private NetworkBehaviour networkBehaviour;

    public GameObject window;
    [SerializeField] private GameObject hoveringKeyObj_P1;
    [SerializeField] private GameObject hoveringKeyObj_P2;

    private GameObject hoveringKeyObj;

    public bool isEnd = false;
    private float timer = 0f;

    private void Awake()
    {
        CheckHandTransform parentScript = GetComponent<CheckHandTransform>();

        if (parentScript != null)
        {
            xr_input = parentScript.xr_input;
            leftHand = parentScript.leftHand;
            rightHand = parentScript.rightHand;
        }
    }

    private void Update()
    {
        if(networkBehaviour != null)
        {
            // 플레이어1일 때
            if(networkBehaviour.OwnerClientId == 0)
            {
                // p1의 호버링 키 오브젝트를 가져온다
                hoveringKeyObj = hoveringKeyObj_P1;
            }
            else
            {
                hoveringKeyObj = hoveringKeyObj_P2;
            }

            if(leftHand != null && rightHand != null)
            {
                // 키 오브젝트와 손의 사이 위치 계산
                float distance_L = Vector3.Distance(leftHand.transform.position, hoveringKeyObj.transform.position);
                float distance_R = Vector3.Distance(rightHand.transform.position, hoveringKeyObj.transform.position);

                if (distance_L < 0.2f || distance_R < 0.2f)
                {
                    timer += Time.deltaTime;

                    if(timer > 2.0f)
                    {
                        window.SetActive(false);
                        isEnd = true;
                        timer = 0f;
                        // 엔딩씬 불러오기
                        Debug.Log("엔딩!");
                    }
                }
            }
        }
    }
}
