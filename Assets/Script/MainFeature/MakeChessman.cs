using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
            if (jar.name.Contains("Jar_FirstHeadKey") || jar.name.Contains("Jar_SecondHeadKey_Two"))
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
                    // 어떤 chessman을 생성할지 결정해서 병합
                    if (jar.name.Contains("Jar_SecondHeadKey_Two"))
                    {
                        CheckDistanceNCreate(jar, merged, chessman2);
                    }
                    else if (jar.name.Contains("Jar_FirstHeadKey"))
                    {
                        CheckDistanceNCreate(jar, merged, chessman1);
                    }
                }
            }
        }
    }

    public override void RequestSpawnMergedObject(Vector3 spawnPos)
    {
        Debug.Log("클라이언트에서 서버에게 생성 요청");
        RequestSpawnMergedObjectServerRpc(spawnPos);
    }

    [ServerRpc]
    void RequestSpawnMergedObjectServerRpc(Vector3 spawnPos)
    {
        GameObject objToSpawn = null;

        // 서버에서도 jarObjects를 확인해야 하므로 구조를 바꾸거나
        // 아래처럼 임시로 jar 찾기
        GameObject[] jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");

        foreach (GameObject jar in jarKeys)
        {
            if (jar.name.Contains("Jar_FirstHead"))
            {
                objToSpawn = chessman1;
            }
            else if (jar.name.Contains("Jar_SecondHead"))
            {
                objToSpawn = chessman2;
            }

            if (objToSpawn != null)
            {
                GameObject spawned = Instantiate(objToSpawn, spawnPos, Quaternion.identity);
                spawned.GetComponent<NetworkObject>().Spawn();
                break;
            }
        }
    }

}
