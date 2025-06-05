using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCage : MonoBehaviour, IResettable
{
    [SerializeField] private BoxCollider cageCol;
    [SerializeField] private Animator openCageAnim;

    public bool isOpen = false;

    // 버튼 ui를 눌렀을 때, 박스 콜라이더 비활성화
    public void DisableCageCollider()
    {
        if (cageCol != null)
        {
            cageCol.enabled = false;
            isOpen = true;
            openCageAnim.SetTrigger("openTrigger");
            Debug.Log("Cage Collider Disabled");
        }
        else
        {
            Debug.LogWarning("Cage Collider is not assigned.");
        }
    }

    // IResettable 인터페이스 구현
    public void ResetTrigger()
    {
        isOpen = false;
    }
}
