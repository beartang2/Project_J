using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button Server_Button;
    [SerializeField] private Button Client_Button;
    [SerializeField] private TMP_InputField JoinCodeInput;
    [SerializeField] private TMP_Text JoinCodeDisplay;

    private void Awake()
    {
        Server_Button.onClick.AddListener(async () => {
            await RelayManager.StartRelayServer();
            JoinCodeDisplay.text = $"Join Code {RelayManager.JoinCode}";
        });

        Client_Button.onClick.AddListener(async () => {
            string code = JoinCodeInput.text;
            await RelayManager.StartRelayClient(code);
        });
    }
}
