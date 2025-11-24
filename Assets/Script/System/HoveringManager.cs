using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoveringManager : MonoBehaviour, IResettable
{
    public bool p1Complete = false;
    public bool p2Complete = false;
    // 플레이어 이름을 가진 오브젝트
    private GameObject player1;
    private GameObject player2;
    // 카메라 오브젝트
    [SerializeField] private GameObject cameraObj;

    [SerializeField] private BGM_Manager bgmManager;

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

            // 플레이어 태그를 가진 오브젝트 찾기
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            // 플레이어 전부 비활성화
            foreach (GameObject player in players)
            {
                if (player.name.Contains("Player0"))
                {
                    player1 = player;
                    player.SetActive(false);
                }
                else if (player.name.Contains("Player1"))
                {
                    player2 = player;
                    player.SetActive(false);
                }
            }

            // 엔딩 카메라 활성화
            cameraObj.SetActive(true);
            bgmManager.audioSource.clip = bgmManager.endingClip;
            bgmManager.audioSource.Play();
        }
    }

    public void ResetTrigger()
    {
        p1Complete = false;
        p2Complete = false;
        
        Debug.Log("HoveringManager 리셋됨");
    }
}
