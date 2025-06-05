using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoveringManager : MonoBehaviour, IResettable
{
    private bool p1Complete = false;
    private bool p2Complete = false;

    //[SerializeField] private GameObject window;

    public void ReportHoverComplete(ulong clientId)
    {
        if (clientId == 0)
        {
            p1Complete = true;
            Debug.Log("P1이 키에 손을 댐!");
        }
        else
        {
            p2Complete = true;
            Debug.Log("P2가 키에 손을 댐!");
        }

        if (p1Complete && p2Complete)
        {
            Debug.Log("두 플레이어 모두 키에 손을 댐! 문이 열립니다.");
            //window.SetActive(false);
            // 엔딩씬 호출
            SceneManager.LoadScene("Ending");
        }
    }

    public void ResetTrigger()
    {
        p1Complete = false;
        p2Complete = false;
        
        Debug.Log("HoveringManager 리셋됨");
    }
}
