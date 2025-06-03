using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum EAudioMixerType { Master, BGM, SFX }

public class InGameSetting : NetworkBehaviour
{
    public static InGameSetting Instance;

    [SerializeField] private Transform startPos;
    private Net_DisconnectHandler dcHandler;

    public XRControllerInput leftController;
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    [SerializeField] private GameObject settingPanelPrefab;
    private Transform settingCanvas;
    private GameObject settingPanelInstance;

    private AudioSource buttonAudioSc;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioMixer audioMixer;
    public Transform playerCamera;
    public float spawnDistance = 1.5f;

    private bool isMenuActive = false;
    private InputDevice leftHandDevice;
    private bool wasXPressed = false;
    private bool isMultiplayerActive = false;

    private void Awake()
    {
        Instance = this;
        buttonAudioSc = GetComponent<AudioSource>();
    }

    private void Start()
    {
        dcHandler = FindObjectOfType<Net_DisconnectHandler>();
        settingCanvas = GameObject.Find("SettingCanvas")?.transform;

        // 무조건 생성 (멀티/싱글 모두 가능하게)
        if (settingPanelPrefab != null && settingCanvas != null)
        {
            settingPanelInstance = Instantiate(settingPanelPrefab, settingCanvas);
            settingPanelInstance.SetActive(false);
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            isMultiplayerActive = true;
        }

        // 오디오 초기값 설정
        float volume;
        if (audioMixer.GetFloat("Master", out volume)) masterSlider.value = Mathf.Pow(10, volume / 20);
        if (audioMixer.GetFloat("BGM", out volume)) bgmSlider.value = Mathf.Pow(10, volume / 20);
        if (audioMixer.GetFloat("SFX", out volume)) sfxSlider.value = Mathf.Pow(10, volume / 20);

        masterSlider.onValueChanged.AddListener(value => SetAudioVolume(EAudioMixerType.Master, value));
        bgmSlider.onValueChanged.AddListener(value => SetAudioVolume(EAudioMixerType.BGM, value));
        sfxSlider.onValueChanged.AddListener(value => SetAudioVolume(EAudioMixerType.SFX, value));
    }

    public override void OnNetworkSpawn()
    {
        // 카메라 세팅
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsLocalPlayer)
                {
                    Camera cam = player.GetComponentInChildren<Camera>();
                    if (cam != null) playerCamera = cam.transform;
                    break;
                }
            }
        }
        else
        {
            playerCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        // 멀티플레이 중이면 오너만 입력 허용
        if (isMultiplayerActive && !IsOwner) return;

        if (!leftHandDevice.isValid)
        {
            var leftHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            if (leftHandDevices.Count > 0)
            {
                leftHandDevice = leftHandDevices[0];
            }
        }

        ulong localId = NetworkManager.Singleton.LocalClientId;

        foreach (var player in FindObjectsOfType<DisableOtherPlayerInput>())
        {
            if (player.OwnerClientId == localId)
            {
                if (leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
                {
                    if (isPressed && !wasXPressed)
                    {
                        Debug.Log("X 버튼 눌림, 메뉴 토글");
                        ToggleSettingsMenu();
                    }
                    wasXPressed = isPressed;
                }
                else
                {
                    wasXPressed = false;
                }
            }
        }
    }

    public void SetAudioVolume(EAudioMixerType audioMixerType, float volume)
    {
        float mixerVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(audioMixerType.ToString(), mixerVolume);
    }

    void ToggleSettingsMenu()
    {
        Debug.Log("ToggleSettingsMenu called");
        isMenuActive = !isMenuActive;

        if (isMenuActive)
        {
            Vector3 forwardPos = playerCamera.position + playerCamera.forward * spawnDistance;
            settingPanelInstance.transform.position = forwardPos;

            Vector3 lookDir = playerCamera.position - forwardPos;
            lookDir.y = 0;
            settingPanelInstance.transform.rotation = Quaternion.LookRotation(-lookDir);

            settingPanelInstance.SetActive(true);
        }
        else
        {
            settingPanelInstance.SetActive(false);
        }
    }

    public void SaveSettingData()
    {
        if (buttonAudioSc != null)
        {
            buttonAudioSc.PlayOneShot(audioClip);
        }
        print("Save");
        settingPanelPrefab.SetActive(false);
    }

    public void Exit()
    {
        if (buttonAudioSc != null)
        {
            buttonAudioSc.PlayOneShot(audioClip);
        }

        if (dcHandler != null)
        {
            dcHandler.playerTransform = this.transform;
        }

        NetworkManager.Singleton.Shutdown();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
