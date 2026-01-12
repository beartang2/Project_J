using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class RelayManager : MonoBehaviour
{
    public static string JoinCode; // 외부에 노출할 Join Code 저장용

    private async void Awake()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("[Relay] Anonymous SignIn 성공");
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // 호스트가 나가면 클라이언트는 로비 씬으로 이동
        if (!NetworkManager.Singleton.IsHost && clientId == NetworkManager.ServerClientId)
        {
            Debug.Log("[RelayManager] 서버 연결 끊김 감지, 로비로 이동");
            SceneManager.LoadScene("Lobby");  // 실제 로비 씬 이름으로 변경
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지용 콜백 해제
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public static async Task StartRelayServer()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(2);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log($"[Relay] 서버 JoinCode: {JoinCode}");

        var relayServerData = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartServer();
    }


    public static async Task StartRelayClient(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }
}
