using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetting : NetworkBehaviour
{
    [SerializeField] private float spawnRange = 5f;
    [SerializeField] private MeshRenderer meshRenderer;

    public List<Color> colors = new List<Color>();

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        // 스폰 위치 랜덤
        transform.position = new Vector3(Random.Range(spawnRange, -spawnRange), 2, Random.Range(spawnRange, -spawnRange));

        // 클라이언트 번호에 따라 Color 리스트에서 순서대로 색상 부여
        meshRenderer.material.color = colors[(int)OwnerClientId];
    }
}
