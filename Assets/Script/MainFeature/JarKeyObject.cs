using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;

public class JarKeyObject : NetworkBehaviour
{
    private bool hasInserted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasInserted || !IsServer) return;

        if (other.tag == "Jar")
        {
            hasInserted = true;

            // 항아리 스크립트를 찾아서 등록 요청
            var jar = other.GetComponent<InsertToJar>();
            if (jar != null)
            {
                jar.InsertKeyServerRpc(NetworkObjectId, name);  // 자기 정보 전달
            }

            // 자기 자신 비활성화 (필요에 따라 처리 방식 다르게 가능)
            gameObject.SetActive(false);
        }
    }
}
