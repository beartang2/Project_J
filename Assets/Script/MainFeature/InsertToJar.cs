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

    [ServerRpc(RequireOwnership = false)]
    public void InsertKeyServerRpc(ulong netId, string name)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netId];
        GameObject obj = netObj.gameObject;

        if (name.Contains("Jar_Key_A"))
        {
            objects1.Add(obj);
            cnt1++;
        }
        else if (name.Contains("Jar_Key_C"))
        {
            objects2.Add(obj);
            cnt2++;
        }

        if (cnt1 == 2 && !is1Init)
        {
            is1Init = true;
            var newObj = Instantiate(resultObj1, transform.position, Quaternion.identity);
            newObj.GetComponent<NetworkObject>().Spawn();
        }
        else if (cnt2 == 2 && !is2Init)
        {
            is2Init = true;
            var newObj = Instantiate(resultObj2, transform.position, Quaternion.identity);
            newObj.GetComponent<NetworkObject>().Spawn();
        }
    }

}
