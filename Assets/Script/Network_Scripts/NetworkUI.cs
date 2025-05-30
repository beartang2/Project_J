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

    private bool isClicked = false;

    private void Awake()
    {
        // 서버 접속
        Server_Button.onClick.AddListener(async () => {
            await RelayManager.StartRelayServer();
            isClicked = true;
        });

        Client_Button.onClick.AddListener(async () => {
            string code = JoinCodeInput.text;
            await RelayManager.StartRelayClient(code);
        });
    }

    // server button을 실수로 두번 클릭해도 joincode가 업데이트 되도록
    private void Update()
    {
        if (isClicked)
        {
            JoinCodeDisplay.text = "Join Code\r\n" + RelayManager.JoinCode;
            isClicked = false;
        }
    }
}
