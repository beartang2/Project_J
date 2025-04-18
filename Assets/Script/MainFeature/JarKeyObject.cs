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

            DisableSelfClientRpc(); // 클라이언트에도 비활성화 요청
            gameObject.SetActive(false); // 서버에서도 비활성화
        }
    }

    [ClientRpc]
    private void DisableSelfClientRpc()
    {
        gameObject.SetActive(false);
    }
}
