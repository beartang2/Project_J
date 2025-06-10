using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum EAudioMixerType { Master, BGM, SFX }

public class InGameSetting : MonoBehaviour
{
    //[SerializeField] private Transform startPos;
    //private Net_DisconnectHandler dcHandler;

    public XRControllerInput leftController;
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    private Transform settingCanvas;
    private GameObject settingPanelInstance;

    [SerializeField] private AudioMixer audioMixer;
    public Transform playerCamera;
    public float spawnDistance = 1.5f;

    private InputDevice leftHandDevice;
    private bool wasXPressed = false;

    private void Start()
    {
        // 자기 Canvas 및 패널 찾기 (자식 기준)
        settingCanvas = transform.Find("SettingCanvas");
        settingPanelInstance = settingCanvas.Find("SettingPanel")?.gameObject;
        settingPanelInstance?.SetActive(false);

        masterSlider = settingPanelInstance.transform.Find("MasterSlider")?.GetComponent<Slider>();
        bgmSlider = settingPanelInstance.transform.Find("BGMSlider")?.GetComponent<Slider>();
        sfxSlider = settingPanelInstance.transform.Find("SFXSlider")?.GetComponent<Slider>();

        SetupSlider(masterSlider, EAudioMixerType.Master, "Master");
        SetupSlider(bgmSlider, EAudioMixerType.BGM, "BGM");
        SetupSlider(sfxSlider, EAudioMixerType.SFX, "SFX");
    }


    private void Update()
    {
        // 멀티플레이 중이면 오너만 입력 허용

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

    private void SetupSlider(Slider slider, EAudioMixerType type, string mixerName)
    {
        if (slider == null) return;

        float volume = 0.7f;
        if (audioMixer.GetFloat(mixerName, out volume))
            slider.value = Mathf.Pow(10, volume / 20);

        slider.onValueChanged.AddListener(value => SetAudioVolume(type, value));
    }


    public void SetAudioVolume(EAudioMixerType audioMixerType, float volume)
    {
        float mixerVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(audioMixerType.ToString(), mixerVolume);
    }

    void ToggleSettingsMenu()
    {
        if (!settingPanelInstance.activeSelf)
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

    public void Exit()
    {
        /*if (dcHandler != null)
        {
            dcHandler.playerTransform = this.transform;
        }*/

        NetworkManager.Singleton.Shutdown();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
