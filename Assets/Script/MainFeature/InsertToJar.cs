using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InsertToJar : MonoBehaviour, IResettable
{
    private GameObject unknownObj;
    private List<GameObject> objects1;
    private List<GameObject> objects2;
    [SerializeField] private GameObject resultObj1;     // 첫 번째 머리
    [SerializeField] private GameObject resultObj2;     // 두 번째 머리
    private bool is1Init = false;
    private bool is2Init = false;
    private int cnt1 = 0;
    private int cnt2 = 0;

    private void Start()
    {
        objects1 = new List<GameObject>();
        objects2 = new List<GameObject>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void InsertKeyServerRpc(ulong netId, string name)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netId];
        GameObject obj = netObj.gameObject;

        if (name.Contains("Jar_Key_A"))
        {
            objects1.Add(obj);
            cnt1++;
            // 오브젝트 효과음
            // 오브젝트 이펙트
        }
        else if (name.Contains("Jar_Key_C"))
        {
            objects2.Add(obj);
            cnt2++;
            // 오브젝트 효과음
            // 오브젝트 이펙트
        }

        if (cnt1 == 2 && !is1Init)
        {
            is1Init = true;
            var newObj = Instantiate(resultObj1, transform.position, Quaternion.identity);
            newObj.GetComponent<NetworkObject>().Spawn();
            // 연금술 성공 효과음
            // 연금술 성공 이펙트
        }
        else if (cnt2 == 2 && !is2Init)
        {
            is2Init = true;
            var newObj = Instantiate(resultObj2, transform.position, Quaternion.identity);
            newObj.GetComponent<NetworkObject>().Spawn();
            // 연금술 성공 효과음
            // 연금술 성공 이펙트
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 다른 오브젝트가 들어왔을때
        if(other.name.Contains("Extra"))
        {
            DisableSelfClientRpc(); // 클라이언트에도 비활성화 요청
            gameObject.SetActive(false); // 서버에서도 비활성화
            // 트리거 초기화
            ResetTrigger();
            // 연금술 실패 효과음
            // 연금술 실패 이펙트
        }
    }

    public void ResetTrigger()
    {
        cnt1 = 0;
        cnt2 = 0;
        is1Init = false;
        is2Init = false;

        Debug.Log("[InsertToJar] 트리거 상태 초기화 완료");
    }

    [ClientRpc]
    private void DisableSelfClientRpc()
    {
        gameObject.SetActive(false);
    }
}
