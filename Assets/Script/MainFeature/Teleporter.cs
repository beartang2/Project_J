using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    // A -> B, B -> A
    private GameObject objects;
    //private Queue<GameObject> teleportObjects;
    Collider boxCol;
    private bool isTeleported = false;
    //float delayTime = 0;
    float yOffset = 0.5f;
    public bool isPlayerPortal = false; // 플레이어 발판인가?
    public bool canPort = false;        // 플레이어가 이동 가능한 상태인가?

    // 도착지점 포지션
    [SerializeField] private GameObject arrivePosObj;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 포탈이고 충돌체가 플레이어라면
        if (isPlayerPortal && other.CompareTag("Player"))
        {
            // 이동 가능한 상태라면
            if(canPort)
            {
                Vector3 newPos = arrivePosObj.transform.position;
                newPos.y += yOffset; // Y축 오프셋 추가
                                     // 도착지점으로 이동
                other.gameObject.transform.position = newPos;

                SetAllTeleportersFalse();
            }
        }
        if(!isTeleported && other.gameObject.tag.Contains("keyObject"))
        {
            Vector3 newPos = arrivePosObj.transform.position;
            newPos.y += yOffset; // Y축 오프셋 추가
            // 도착지점으로 이동
            other.gameObject.transform.position = newPos;

            // 전체 Teleporter의 canPort를 false로 설정
            SetAllTeleportersFalse();
        }
    }

    // 모든 Teleporter의 canPort를 false로 변경하는 함수
    public void SetAllTeleportersFalse()
    {
        Teleporter[] teleporters = FindObjectsOfType<Teleporter>(); // 모든 Teleporter 찾기

        foreach (Teleporter tele in teleporters)
        {
            tele.canPort = false;
        }
    }

    // 콜라이더에 들어온지 2초가 되고 Exit하지 않으면, A->B, B->A 로 좌표 이동
    // 콜라이더 내 좌표값 구하기

    /*
    private void Awake()
    {
        //teleportObjects = new Queue<GameObject>();
        boxCol = gameObject.GetComponent<Collider>();
        isTeleported = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        delayTime = 0f;
    }
    private void OnTriggerStay(Collider other)
    {
        // 콜라이더에 들어온 오브젝트를 큐에 담음
        //teleportObjects.Enqueue(other.gameObject);
        //delayTime += Time.deltaTime;

        if (delayTime >= 3.0f)
        {
            if (!isTeleported && other.gameObject.tag == "keyObjects")
            {
                StartCoroutine(DelayTeleport(OtherTeleportObj.transform.position));
            }
            isTeleported = false;
        }        
    }

    private Vector3 ColliderPosition(Collider other)
    {
        return other.ClosestPoint(transform.position);
    }

    IEnumerator DelayTeleport(Vector3 objPos)
    {
        yield return new WaitForSeconds(2f);

        //objects = teleportObjects.Peek();
        objects.transform.position = objPos;
        isTeleported = true;


        yield return new WaitForSeconds(2f);
        isTeleported = false;
        
    }*/
}
