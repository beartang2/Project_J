using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckChessPosition : CheckHandTransform
{
    [SerializeField] private int num;
    [SerializeField] private GameObject origin_chessman;
    private float timer;

    private void Awake()
    {
        origin_chessman.SetActive(false);
        timer = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        timer += Time.deltaTime;

        // 충돌체가 키 오브젝트일 때
        if(timer > 2.5f && (!xr_input.isLPressed && !xr_input.isRPressed) &&
            num == 1 && other.gameObject.CompareTag("chessKey1"))
        {
            other.gameObject.SetActive(false);
            origin_chessman.SetActive(true);
            timer = 0f; // 타이머 초기화
        }

        if(timer > 2.5f && (!xr_input.isLPressed && !xr_input.isRPressed) &&
            num == 2 && other.gameObject.CompareTag("chessKey2"))
        {
            other.gameObject.SetActive(false);
            origin_chessman.SetActive(true);
            timer = 0f; // 타이머 초기화
        }
    }

}
