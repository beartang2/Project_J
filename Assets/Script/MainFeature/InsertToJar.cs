using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InsertToJar : MonoBehaviour
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

    private void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(other.tag == "jar_keyObject")
        {
            Debug.Log("키 오브젝트 들어옴");
            // 리스트에 담기

            if (other.name.Contains("Jar_Key_A"))
            {
                objects1.Add(other.gameObject);
                cnt1++;
            }
            else if(other.name.Contains("Jar_Key_C"))
            {
                objects2.Add(other.gameObject);
                cnt2++;
            }

            if(cnt1 == 2 && !is1Init)
            {
                // 키 오브젝트 생성
                GameObject newObject = Instantiate(resultObj1, gameObject.transform.position, Quaternion.identity);
                newObject.GetComponent<NetworkObject>().Spawn();
                is1Init = true;

                Debug.Log(objects1.Count);
                for (int i = 0; i < cnt1; i++)
                {
                    // 오브젝트 개수만큼 비활성화
                    NetworkObject netObj = objects1[i].GetComponent<NetworkObject>();
                    if (netObj.IsSpawned)
                    {
                        netObj.Despawn();
                    }
                }
            }
            else if(cnt2 == 2 && !is2Init)
            {
                // 키 오브젝트 생성
                GameObject newObject = Instantiate(resultObj2, gameObject.transform.position, Quaternion.identity);
                newObject.GetComponent<NetworkObject>().Spawn();
                is2Init = true;

                Debug.Log(objects2.Count);
                for (int i = 0; i < cnt2; i++)
                {
                    // 오브젝트 개수만큼 비활성화
                    NetworkObject netObj = objects2[i].GetComponent<NetworkObject>();
                    if (netObj.IsSpawned)
                    {
                        netObj.Despawn();
                    }
                }
            }
        }
    }
}
