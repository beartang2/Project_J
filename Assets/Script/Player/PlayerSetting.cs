using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetting : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer bodyRenderer;
    [SerializeField] private SkinnedMeshRenderer headRenderer;
    [SerializeField] private SkinnedMeshRenderer objRenderer;

    public List<Mesh> bodyMeshes = new List<Mesh>();
    public List<Mesh> headMeshes = new List<Mesh>();
    public List<Mesh> objMeshes = new List<Mesh>();

    public List<Material> bodyMaterials = new List<Material>();
    public List<Material> headMaterials = new List<Material>();
    public List<Material> objMaterials = new List<Material>();

    private void Awake()
    {
        if (bodyRenderer == null)
            bodyRenderer = transform.Find("Body")?.GetComponent<SkinnedMeshRenderer>();
        if (headRenderer == null)
            headRenderer = transform.Find("Head")?.GetComponent<SkinnedMeshRenderer>();
        if (objRenderer == null)
            Debug.LogWarning("PlayerSetting: objRenderer is not assigned. Please assign it in the inspector.");
    }

    public override void OnNetworkSpawn()
    {
        int index = (int)OwnerClientId;

        // 리스트 크기 확인
        if (index >= bodyMeshes.Count || index >= bodyMaterials.Count ||
            index >= headMeshes.Count || index * 2 + 1 >= headMaterials.Count ||
            index >= objMeshes.Count || index >= objMaterials.Count)
        {
            Debug.LogWarning($"PlayerSetting: Index {index} out of range.");
            return;
        }

        // 메시 적용
        bodyRenderer.sharedMesh = bodyMeshes[index];
        headRenderer.sharedMesh = headMeshes[index];
        objRenderer.sharedMesh = objMeshes[index];

        // 머티리얼 적용
        bodyRenderer.material = bodyMaterials[index];
        // head 머티리얼 2개
        headRenderer.materials = new Material[]
        {
            headMaterials[index * 2],
            headMaterials[index * 2 + 1]
        };
        objRenderer.material = objMaterials[index];
    }
}
