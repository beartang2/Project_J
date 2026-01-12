using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetting : NetworkBehaviour
{
    //[SerializeField] private float spawnRange = 5f;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    //[SerializeField] private SkinnedMeshRenderer meshRenderer_p2;
    //private GameObject player2Obj;
    [SerializeField] private GameObject leftHandModelPrefab;
    [SerializeField] private GameObject rightHandModelPrefab;

    public List<Mesh> otherPlayerMesh = new List<Mesh>();   // 교체할 메시
    public List<Material> colors = new List<Material>();

    private void Awake()
    {
        meshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

        // 만약 "Player2"라는 이름을 가진 오브젝트가 존재한다면
        //player2Obj = GameObject.Find("Player2");
    }

    public override void OnNetworkSpawn()
    {
        // 현재 머티리얼 배열을 복사
        Material[] mats = meshRenderer.materials;
        
        if (IsServer)
        {
            // 서버 플레이어는 노란색 (colors[0])
            meshRenderer.sharedMesh = otherPlayerMesh[0]; // 서버 플레이어는 첫 번째 메시 사용
            
            // 머리 제외하고 나머지만 변경
            mats[1] = colors[0];
            mats[2] = colors[2];
            // 적용
            meshRenderer.materials = mats;

            // 본인이 아닌 플레이어(=다른 클라이언트)일 경우에만 손 모델 붙이기
            if (!IsOwner)
            {
                AttachHandModels();
            }
        }
        if (OwnerClientId > 0)
        {
            // 클라이언트 색상 부여
            meshRenderer.sharedMesh = otherPlayerMesh[1]; // 클라이언트 플레이어는 두 번째 메시 사용
            
            // 머리 제외하고 나머지만 변경
            mats[1] = colors[1];
            mats[2] = colors[3];
            // 적용
            meshRenderer.materials = mats;
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

    private void AttachHandModels()
    {
        // Player_with_model 루트 오브젝트 찾기
        Transform root = transform;

        while (root.parent != null)
            root = root.parent;

        Transform leftModelParent = root.Find("Camera Offset/Left Controller/[Left Controller] Model Parent");
        Transform rightModelParent = root.Find("Camera Offset/Right Controller/[Right Controller] Model Parent");

        if (leftModelParent != null && rightModelParent != null)
        {
            Instantiate(leftHandModelPrefab, leftModelParent);
            Instantiate(rightHandModelPrefab, rightModelParent);
        }
        else
        {
            Debug.LogWarning("손 모델 부착 대상 트랜스폼을 찾을 수 없습니다!");
        }
    }

}
