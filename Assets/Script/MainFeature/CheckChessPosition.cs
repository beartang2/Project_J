using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckChessPosition : CheckHandTransform
{
    [SerializeField] private int num;
    [SerializeField] private GameObject origin_chessman;

    private void Awake()
    {
        origin_chessman.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌체가 키 오브젝트일 때
        if(num == 1 && other.gameObject.CompareTag("chessKey1"))
        {
            other.gameObject.SetActive(false);
            origin_chessman.SetActive(true);
        }

        if(num == 2 && other.gameObject.CompareTag("chessKey2"))
        {
            other.gameObject.SetActive(false);
            origin_chessman.SetActive(true);
        }
    }

}
