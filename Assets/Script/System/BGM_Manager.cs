using UnityEngine;

public class BGM_Manager : MonoBehaviour
{
    public static BGM_Manager Instance;

    public AudioSource audioSource;
    public AudioClip room1Clip;
    public AudioClip room2Clip;
    public AudioClip room3Clip;
    public AudioClip room4Clip;
    public AudioClip endingClip;

    // PlayerMovement or PlayerInit.cs
    [SerializeField] private SpawnPoint[] spawnPoints; // 할당 필요

    private Net_PlayerSpawner playerSpawner;

    private void Awake()
    {
        playerSpawner = FindObjectOfType<Net_PlayerSpawner>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(SpawnArea area)
    {
        AudioClip selectedClip = null;

        switch (area)
        {
            case SpawnArea.Room1:
                selectedClip = room1Clip;
                break;
            case SpawnArea.Room2:
                selectedClip = room2Clip;
                break;
            case SpawnArea.Room3:
                selectedClip = room3Clip;
                break;
            case SpawnArea.Room4:
                selectedClip = room4Clip;
                break;
        }

        if (audioSource.clip != selectedClip)
        {
            audioSource.clip = selectedClip;
            audioSource.Play();
        }
    }

    // 플레이어가 처음 시작할 때 BGM을 재생
    public void PlayBGM()
    {
        if (playerSpawner.pMoved)
        {
            PlayBGM(SpawnArea.Room1);
        }
    }
}
