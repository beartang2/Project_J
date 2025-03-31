using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertToJar : MonoBehaviour
{
    private GameObject unknownObj;
    private List<GameObject> objects;
    [SerializeField] private GameObject resultObj;
    private bool isInit = false;
    private int cnt = 0;

    private void Start()
    {
        isInit = false;
        cnt = 0;
        objects = new List<GameObject>();
    }

    private void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(other.tag == "keyObjects")
        {
            Debug.Log("키 오브젝트 들어옴");
            cnt++;

            // 리스트에 담기
            objects.Add(other.gameObject);

            if(cnt == 2 && !isInit)
            {
                // 키 오브젝트 생성
                Instantiate(resultObj, gameObject.transform.position, Quaternion.identity);
                isInit = true;

                Debug.Log(objects.Count);
                for (int i = 0; i < cnt; i++)
                {
                    // 오브젝트 개수만큼 비활성화
                    objects[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
