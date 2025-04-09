using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetting : NetworkBehaviour
{
    [SerializeField] private float spawnRange = 5f;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshRenderer meshRenderer_p2;
    //private GameObject player2Obj;

    public List<Color> colors = new List<Color>();

    private void Awake()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        // 만약 "Player2"라는 이름을 가진 오브젝트가 존재한다면
        //player2Obj = GameObject.Find("Player2");
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 서버 플레이어는 노란색 (colors[0])
            meshRenderer.material.color = colors[0];
        }
        if (OwnerClientId > 0)
        {
            // 클라이언트 번호에 따라 Color 리스트에서 순서대로 색상 부여
            meshRenderer.material.color = colors[1];
        }
        /*
        if (player2Obj != null)
        {
            // meshRenderer_p2에 Player2 오브젝트의 MeshRenderer 컴포넌트를 가져온다
            meshRenderer_p2 = player2Obj.GetComponentInChildren<MeshRenderer>();

            // Player2에게 하늘색을 부여
            meshRenderer_p2.material.color = colors[1];
        }*/
    }
}
