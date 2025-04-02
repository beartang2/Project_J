using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeChessman : CheckHandTransform
{
    private List<GameObject> jarObjects = new List<GameObject>();
    [SerializeField] private GameObject chessman1; // Jar_HeadKey1과 합쳐질 경우 생성
    [SerializeField] private GameObject chessman2; // Jar_HeadKey2와 합쳐질 경우 생성

    private void Update()
    {
        GameObject[] mergedKeys = GameObject.FindGameObjectsWithTag("merged_key");
        GameObject[] jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");

        jarObjects.Clear(); // 리스트 초기화

        // jarObjects 리스트에 Jar_HeadKey1 또는 Jar_HeadKey2 추가
        foreach (GameObject jar in jarKeys)
        {
            if (jar.name.Contains("Jar_HeadKey") || jar.name.Contains("Jar_HeadKey2"))
            {
                jarObjects.Add(jar);
            }
        }

        if (jarObjects.Count > 0 && mergedKeys.Length > 0)
        {
            foreach (GameObject jar in jarObjects)
            {
                foreach (GameObject merged in mergedKeys)
                {
                    // CheckDistanceNCreate()에 맞게 어떤 chessman을 생성할지 결정
                    if (jar.name == "Jar_HeadKey")
                    {
                        CheckDistanceNCreate(jar, merged, chessman1);
                    }
                    else if (jar.name == "Jar_HeadKey2")
                    {
                        CheckDistanceNCreate(jar, merged, chessman2);
                    }
                }
            }
        }
    }
}
