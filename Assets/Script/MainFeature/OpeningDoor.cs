using Meta.XR.ImmersiveDebugger.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

                        if(gameObject.name.Contains("1P"))
                        {
                            SetTeleporterCanPortByTag("Teleporter_A", true); // P1용
                        }
                        else
                        {
                            SetTeleporterCanPortByTag("Teleporter_B", true); // P2용
                        }
                    }
                }
            }
        }

        if(jar_key != null && merged_key != null)
        {
            SetTeleporterCanPortByTag("", true);
        }
    }

    // 텔레포트 활성화
    public void SetTeleporterCanPortByTag(string tag, bool value)
    {
        GameObject[] teleporters = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject tp in teleporters)
        {
            Debug.Log(tp);
            if (tp.GetComponent<Teleporter>() != null)
            {
                tp.GetComponent<Teleporter>().canPort = value;
                Debug.Log(tag + " Tag, " + tp + " Object canPort = true");
            }
        }
    }
}
