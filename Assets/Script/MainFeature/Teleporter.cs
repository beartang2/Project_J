using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResettingManager;

public class Teleporter : MonoBehaviour, IResettable
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
    [SerializeField] private GameObject arrivePosObj2;

    private void OnTriggerEnter(Collider other)
    {
        // 만약 이름이 Player1인 오브젝트가 teleporter에 들어오고, arrivePosObj2가 null이 아니면
        if (arrivePosObj2 != null && other.name.Contains("Player1"))
        {
            Vector3 newPos = arrivePosObj2.transform.position;
            newPos.y += yOffset;
            other.transform.position = newPos;

            ResetTrigger();
        }

        if (isPlayerPortal && other.CompareTag("Player") && canPort)
        {
            Vector3 newPos = arrivePosObj.transform.position;
            newPos.y += yOffset;
            other.transform.position = newPos;

            ResetTrigger();

            // 일정 시간 후 다시 포탈을 활성화
            StartCoroutine(ReactivateTeleportersAfterDelay(4f)); // 3~5초 조절 가능
        }

        if (!isTeleported && (other.tag.Contains("key")|| other.tag.Contains("Object")))
        {
            Vector3 newPos = arrivePosObj.transform.position;
            newPos.y += yOffset;
            other.transform.position = newPos;
        }
    }

    // Teleporter의 canPort를 false로 변경하는 함수
    public void ResetTrigger()
    {
        canPort = false;
        isTeleported = false;
        Debug.Log("Teleporter 리셋됨");
    }

    IEnumerator ReactivateTeleportersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Teleporter[] teleporters = FindObjectsOfType<Teleporter>();
        foreach (Teleporter tele in teleporters)
        {
            tele.canPort = true;
        }

        Debug.Log("포탈 재활성화 완료");
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
