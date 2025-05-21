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

    [SerializeField] private Transform startPos;  // 시작 위치
    private Net_DisconnectHandler dcHandler;

    public XRControllerInput leftController;
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public GameObject settingPanel;
    //public Image soundWaveImg;
    //public List<Sprite> soundWaves;
    private AudioSource buttonAudioSc;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioMixer audioMixer;
    public Transform playerCamera;        // 카메라 위치 (XR Origin 안의 Main Camera)
    public float spawnDistance = 1.5f;    // 카메라로부터 얼마나 앞에 생성할지

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
    
        // 네트워크가 활성화되었는지 확인
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            isMultiplayerActive = true;
        }
        // 슬라이더 초기값 설정 (AudioMixer에서 현재 값을 불러와 반영)
        float volume;
        if (audioMixer.GetFloat("Master", out volume)) masterSlider.value = Mathf.Pow(10, volume / 20);
        if (audioMixer.GetFloat("BGM", out volume)) bgmSlider.value = Mathf.Pow(10, volume / 20);
        if (audioMixer.GetFloat("SFX", out volume)) sfxSlider.value = Mathf.Pow(10, volume / 20);

        // 슬라이더 값 변경 시 이벤트 연결
        masterSlider.onValueChanged.AddListener(value => SetAudioVolume(EAudioMixerType.Master, value));
        bgmSlider.onValueChanged.AddListener(value => SetAudioVolume(EAudioMixerType.BGM, value));
        sfxSlider.onValueChanged.AddListener(value => SetAudioVolume(EAudioMixerType.SFX, value));
    }
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            // 로컬 플레이어의 카메라 찾기
            foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsLocalPlayer)
                {
                    Camera cam = player.GetComponentInChildren<Camera>(); // 혹은 필요한 위치에 따라 조정
                    if (cam != null) playerCamera = cam.transform;
                    break;
                }
            }
        }
        else
        {
            // 호스트이거나 싱글 플레이일 때
            playerCamera = Camera.main.transform;
        }

        if (!IsOwner) return;

        if (!leftHandDevice.isValid)
        {
            var leftHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            if (leftHandDevices.Count > 0)
            {
                leftHandDevice = leftHandDevices[0];
            }
        }

        // 버튼 입력 감지
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
        {
            // 버튼을 눌렀을 때 (눌림 순간만 감지)
            if (isPressed && !wasXPressed)
            {
                ToggleSettingsMenu();
            }

            // 현재 프레임 상태 저장
            wasXPressed = isPressed;
        }
        else
        {
            // 버튼 값을 못 받으면 false 처리
            wasXPressed = false;
        }
    }

    private void Update()
    {
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

        // 버튼 입력 감지
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
        {
            // 버튼을 눌렀을 때 (눌림 순간만 감지)
            if (isPressed && !wasXPressed)
            {
                ToggleSettingsMenu();
            }

            // 현재 프레임 상태 저장
            wasXPressed = isPressed;
        }
        else
        {
            // 버튼 값을 못 받으면 false 처리
            wasXPressed = false;
        }
    }


        public void SetAudioVolume(EAudioMixerType audioMixerType, float volume)
    {
        // 오디오 믹서의 값은 -80 ~ 0까지이기 때문에 0.0001 ~ 1의 Log10 * 20을 한다.
        float mixerVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(audioMixerType.ToString(), mixerVolume);
    }

    void ToggleSettingsMenu()
    {
        isMenuActive = !isMenuActive;

        if (isMenuActive)
        {
            // 카메라 앞 spawnDistance만큼 위치
            Vector3 forwardPos = playerCamera.position + playerCamera.forward * spawnDistance;

            settingPanel.transform.position = forwardPos;

            // 카메라를 바라보도록 회전 (수평만)
            Vector3 lookDir = playerCamera.position - forwardPos;
            lookDir.y = 0;
            settingPanel.transform.rotation = Quaternion.LookRotation(-lookDir);

            settingPanel.SetActive(true);
        }
        else
        {
            settingPanel.SetActive(false);
        }
    }

    public void SaveSettingData()
    {
        if (buttonAudioSc != null)
        {
            buttonAudioSc.PlayOneShot(audioClip);
        }
        print("Save");
        settingPanel.SetActive(false);
    }

    public void Exit()
    {
        if (buttonAudioSc != null)
        {
            buttonAudioSc.PlayOneShot(audioClip);
        }

        // disconnectHandler에 연결
        if (dcHandler != null)
        {
            dcHandler.playerTransform = this.transform;
        }

        // 네트워크 연결 종료
        NetworkManager.Singleton.Shutdown();

        // 게임 종료
#if UNITY_EDITOR //전처리기로 유니티 에디터가 실행중일때 플레이를 멈추도록함.
        UnityEditor.EditorApplication.isPlaying = false; //어플리케이션 플레이를 false로 함.
#else //유니티에디터가 실행중이 아닐때 작동
                        Application.Quit(); //어플리케이션을 종료
#endif
    }
}