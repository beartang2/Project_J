using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningDoor : MonoBehaviour
{
    [SerializeField] private Teleporter teleporterScript;
    private GameObject[] keyObj;
    private GameObject jar_key;
    private GameObject merged_key;

    private void Update()
    {
        keyObj = GameObject.FindGameObjectsWithTag("Key");
        jar_key = GameObject.Find("Jar_Key");
        merged_key = GameObject.Find("Merged_Key");

        if(keyObj != null)
        {
            foreach(GameObject key in keyObj)
            {
                if(key.activeSelf)
                {
                    // 오브젝트 사이 위치 계산
                    float distance = Vector3.Distance(key.transform.position, gameObject.transform.position);

                    if (distance < 0.2f)
                    {
                        key.SetActive(false);
                        SetAllTeleportersTrue();
                    }
                }
            }
        }

        if(jar_key != null && merged_key != null)
        {
            SetAllTeleportersTrue();
        }
    }

    // 모든 Teleporter의 canPort를 true로 변경하는 함수
    public void SetAllTeleportersTrue()
    {
        Teleporter[] teleporters = FindObjectsOfType<Teleporter>(); // 모든 Teleporter 찾기
        foreach (Teleporter tele in teleporters)
        {
            tele.canPort = true;
        }
    }
}
