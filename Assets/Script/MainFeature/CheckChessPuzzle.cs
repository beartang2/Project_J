using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckChessPuzzle : MonoBehaviour
{
    [SerializeField] private GameObject chessman1;
    [SerializeField] private GameObject chessman2;
    private bool doOnce = false;

    private void Update()
    {
        if(!doOnce && chessman1.activeInHierarchy == true && chessman2.activeInHierarchy == true)
        {
            SetAllTeleportersTrue();
            doOnce = true;
        }
    }

    // 모든 Teleporter의 canPort를 true로 변경하는 함수
    public void SetAllTeleportersTrue()
    {
        Teleporter[] teleporters = FindObjectsOfType<Teleporter>(); // 모든 Teleporter 찾기
        foreach (Teleporter tele in teleporters)
        {
            tele.canPort = true;
            Debug.Log("chess canPort = true");
        }
    }
}
