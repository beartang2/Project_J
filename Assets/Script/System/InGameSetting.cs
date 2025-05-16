using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum EAudioMixerType { Master, BGM, SFX }

public class InGameSetting : MonoBehaviour
{
    public static InGameSetting Instance;

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
    private int xButtonCount = 0; // X 버튼이 눌린 횟수

    private void Awake()
    {
        Instance = this;
        buttonAudioSc = GetComponent<AudioSource>();
    }

    private void Start()
    {
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

    private void Update()
    {
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
        if(buttonAudioSc != null)
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
        // 게임 종료
#if UNITY_EDITOR //전처리기로 유니티 에디터가 실행중일때 플레이를 멈추도록함.
        UnityEditor.EditorApplication.isPlaying = false; //어플리케이션 플레이를 false로 함.
#else //유니티에디터가 실행중이 아닐때 작동
                Application.Quit(); //어플리케이션을 종료
#endif
    }
}
