using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningDoor : MonoBehaviour
{
    [SerializeField] private Teleporter teleporterScript;
    GameObject keyObj;

    private void Update()
    {
        keyObj = GameObject.FindGameObjectWithTag("Key");

        if(keyObj != null)
        {
            // 오브젝트 사이 위치 계산
            float distance = Vector3.Distance(keyObj.transform.position, gameObject.transform.position);
            
            if(distance < 0.2f)
            {
                keyObj.SetActive(false);
                teleporterScript.canPort = true;
            }

        }
    }
}
