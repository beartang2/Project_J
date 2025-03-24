using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button Server_Button;
    [SerializeField] private Button Host_Button;
    [SerializeField] private Button Client_Button;

    private void Awake()
    {
        //Lamda expression / Delegate
        Server_Button.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });

        //Lamda expression / Delegate
        Host_Button.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });

        //Lamda expression / Delegate
        Client_Button.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
}
