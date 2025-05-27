using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MakeChessman : CheckHandTransform
{
    private List<GameObject> jarObjects = new List<GameObject>();
    [SerializeField] private GameObject chessman1; // Jar_HeadKey1과 합쳐질 경우 생성
    [SerializeField] private GameObject chessman2; // Jar_HeadKey2와 합쳐질 경우 생성
    private MergeObjects mergedSc;
    [SerializeField] private GameObject[] mergedKeys;    // 몸통 키 (병합)
    [SerializeField] private GameObject[] jarKeys;       // 헤드 키 (항아리)

    private int mergedKeyCount = 0; // 병합된 키 개수
    private int isMade = 0; // 체스말 생성 여부

    private void Awake()
    {
        mergedSc = gameObject.GetComponent<MergeObjects>();
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(DelayedFindObjects());
    }

    private IEnumerator DelayedFindObjects()
    {
        // 약간의 여유 시간 대기 (스폰 완료 대기)
        yield return new WaitForSeconds(0.2f);

        jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");
    }

    private void Update()
    {
        if (mergedKeyCount < 2 && mergedSc.isMerged)
        {
            mergedKeys = GameObject.FindGameObjectsWithTag("merged_key");
            Debug.Log("병합된 키 개수: " + mergedKeys.Length);
            mergedKeyCount++;
        }

        if (mergedKeys != null && mergedKeyCount > 0)
        {
            jarKeys = GameObject.FindGameObjectsWithTag("jar_keyObject");

            foreach (GameObject jar in jarKeys)
            {
                if (jar.name.Contains("Jar_FirstHeadKey") || jar.name.Contains("Jar_SecondHeadKey_Two"))
                {
                    Debug.Log("Jar_HeadKey1 또는 Jar_HeadKey2 발견");
                    jarObjects.Add(jar);
                }
            }
        }

        // 체스말 병합 조건
        if (isMade < 2 && jarObjects.Count > 0 && mergedKeys.Length > 0)
        {
            foreach (GameObject jar in jarObjects)
            {
                foreach (GameObject merged in mergedKeys)
                {
                    if (jar.name.Contains("Jar_SecondHeadKey_Two"))
                    {
                        CheckDistanceNCreate(jar, merged, chessman2);
                        Debug.Log("Jar_SecondHeadKey_Two 병합");
                        isMade++;
                    }
                    else if (jar.name.Contains("Jar_FirstHeadKey"))
                    {
                        CheckDistanceNCreate(jar, merged, chessman1);
                        Debug.Log("Jar_FirstHeadKey 병합");
                        isMade++;
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
