using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;//마샬링을 위한 어셈블리

/// <summary>
/// 상수 클래스
/// </summary>
static public class Constants
{
    public const int HEADER_SIZE = 4;//헤더 사이즈는 6(ID:ushort + Size:int)
    public const int MAX_NAME_BYTE = 32;//이름의 최대 바이트 수 : 한글10글자, 영어숫자32글자
    public const int MAX_MSG_BYTE = 128;
}

/// <summary>
/// 클라이언트 클래스
/// </summary>
public class ServerClient
{
    public TcpClient clientSocket;//클라이언트 소켓(통신 도구)
    public string clientname;//클라이언트 이름
    public Vector3 quaternion;//클라이언트 위치 변수이름 변경, 벡터3->벡터4

    public ServerClient(TcpClient clientSocket, string clientname = "클라이언트", Vector4 quaternion = default)//벡터3->벡터4
    {
        this.clientSocket = clientSocket;
        this.clientname = clientname;
        this.quaternion = quaternion;//변수 이름 변경
    }
}

public class NetworkManager_Server : MonoBehaviour
{
    //쓰레드
    private Thread tcpListenerThread;

    //서버의 소켓
    private TcpListener tcpListener;

    //클라이언트
    private ServerClient client;

    //아이피, 포트
    public string ip;
    public int port;

    //서버 상태
    private bool serverReady;

    //통신 메시지 읽고 쓰기 도구
    private NetworkStream stream;

    //로그
    public Text ServerLog;//ui
    private List<string> logList;//data

    //전송 메시지
    public InputField Text_Input;

    //클라이언트 기능 UI
    public GameObject ButtonConnect;
    public GameObject ButtonDisConnect;
    public GameObject ClientFunctionUI;

    //받은 데이터 저장공간
    byte[] buffer;
    //받은 데이터가 잘릴 경우를 대비하여 임시버퍼에 저장하여 관리
    byte[] tempBuffer;//임시버퍼
    bool isTempByte;//임시버퍼 유무
    int nTempByteSize;//임시버퍼의 크기

    //받은 데이터 처리 공간
    private Queue<stChangeInfoMsg> receive_changeInfo_MSG = new Queue<stChangeInfoMsg>();

    //보내는 메시지 저장공간
    byte[] sendMessage;

    /// <summary>
    /// 헤더 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential/*들어오는순서대로(Queue)*/, Pack = 1/*데이터를 읽을 단위*/)]
    public struct stHeader
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 1/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 1/*ushort size*/)]
        public UInt16 PacketSize; // 나머지부분 메시지 크기
    }
    /// <summary>
    /// 헤더 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stHeader HeaderfromByte(byte[] arr)
    {
        //구조체 초기화
        stHeader str = default(stHeader);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stHeader)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }

    /// <summary>
    /// 헤더 구조체 마샬링 함수(구조체 -> 바이트)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static byte[] GetHeaderToByte(stHeader str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stChangeInfoMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 1/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 1/*ushort size*/)]
        public UInt16 PacketSize; // 나머지부분 메시지 크기

        [MarshalAs(UnmanagedType.ByValTStr/*string*/, SizeConst = (int)(Constants.MAX_NAME_BYTE)/*클라이언트 이름의 최대 바이트*/)]
        public string strClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.ByValArray/*float*/, SizeConst = 3)]
        public float[] quaternion; // 클라이언트 위치 XYZ좌표
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stChangeInfoMsg ChangeInfoMsgfromByte(byte[] arr)
    {
        //구조체 초기화
        stChangeInfoMsg str = default(stChangeInfoMsg);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stChangeInfoMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }

    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(구조체 -> 바이트)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetChangeInfoMsgToByte(stChangeInfoMsg str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 전송 문자열 메시지 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendMessageMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 1/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 1/*ushort size*/)]
        public UInt16 PacketSize; // 나머지부분 메시지 크기

        [MarshalAs(UnmanagedType.ByValTStr/*ushort*/, SizeConst = (int)(Constants.MAX_MSG_BYTE)/*ushort size*/)]
        public string strSendMessage; // 메시지 ID
    }

    /// <summary>
    /// 전송 메시지 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendMessageMsg SendMessageMsgfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendMessageMsg str = default(stSendMessageMsg);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendMessageMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }

    /// <summary>
    /// 전송 메시지 구조체 마샬링 함수(구조체 -> 바이트)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendMessageMsgToByte(stSendMessageMsg str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    // Start is called before the first frame update
    void Start()
    {
        //로그 초기화
        logList = new List<string>();

        //데이터 저장공간 초기화
        buffer = new byte[1024];
        //임시버퍼 초기화
        tempBuffer = new byte[1024];
        isTempByte = false;
        nTempByteSize = 0;

        //보내는 메시지 초기화
        sendMessage = new byte[1024];
    }

    // Update is called once per frame
    void Update()
    {
        //받은 데이터가 있는경우(내 정보 확인)
        if (receive_changeInfo_MSG.Count > 0)
        {
            //차례대로 뽑아낸다.
            stChangeInfoMsg CreateObjMsg = receive_changeInfo_MSG.Dequeue();

            //데이터를 넣는다.
            client.clientname = CreateObjMsg.strClientName;
            client.quaternion.x = CreateObjMsg.quaternion[0];
            client.quaternion.y = CreateObjMsg.quaternion[1];
            client.quaternion.z = CreateObjMsg.quaternion[2];

            //클라이언트 정보 갱신
            UpdatedClientInfo();
        }

        //로그리스트에 쌓였다면
        if (logList.Count > 0)
        {
            //배출
            WriteLog(logList[0]);
            logList.RemoveAt(0);
        }

        //서버 상태에 따라 클라이언트 버튼 활성화/비활성화
        ButtonConnect.SetActive(!serverReady);
        ButtonDisConnect.SetActive(!serverReady);
        ClientFunctionUI.SetActive(!serverReady);
    }

    /// <summary>
    /// 서버 생성 버튼
    /// </summary>
    public void ServerCreate()
    {
        //ip, port 설정
        port = int.Parse(GameObject.Find("Text_Port").GetComponent<InputField>().text);

        // TCP서버 배경 스레드 시작
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequeset));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    /// <summary>
    /// 서버 쓰레드 시작
    /// </summary>
    private void ListenForIncommingRequeset()
    {
        try
        {
            // 소켓 생성
            tcpListener = new TcpListener(IPAddress.Any/*서버에 접속 가능한 IP*/, port);
            tcpListener.Start();

            // 서버 상태 ON
            serverReady = true;

            // 로그 기록
            logList.Add("[시스템] 서버 생성(port:" + port + ")");

            // 데이터 리시브 항시 대기(Update)
            while (true)
            {
                // 서버를 연적이 없다면
                if(!serverReady)
                    break;

                //연결 시도중인 클라이언트 확인
                if(tcpListener != null && tcpListener.Pending())
                {
                    //클라이언트 저장
                    client = new ServerClient(tcpListener.AcceptTcpClient());
                    stream = client.clientSocket.GetStream();

                    //Send a message to everyone, say someone has connected
                    Send("클라이언트 접속!");
                }

                //접속된 클라이언트 존재시 상호작용 처리
                if(client != null)
                {
                    //클라이언트 접속 종료시
                    if (!IsConnected(client.clientSocket))
                    {
                        //클라이언트 정보 삭제
                        client.clientSocket.Close();
                        client = null;

                        //로그 기록
                        logList.Add("[시스템] 클라이언트 접속 해제");

                        continue;
                    }
                    //클라이언트 메시지 처리
                    else
                    {
                        //메시지가 들어왔다면
                        if (stream.DataAvailable)
                        {
                            //메시지 저장 공간 초기화
                            Array.Clear(buffer, 0, buffer.Length);

                            //메시지를 읽는다.
                            int messageLength = stream.Read(buffer, 0, buffer.Length);

                            //실제 처리하는 버퍼
                            byte[] pocessBuffer = new byte[messageLength + nTempByteSize];//지금 읽어온 메시지에 남은 메시지의 사이즈를 더해서 처리할 버퍼 생성
                            //남았던 메시지가 있다면
                            if (isTempByte)
                            {
                                //앞 부분에 남았던 메시지 복사
                                Array.Copy(tempBuffer, 0, pocessBuffer, 0, nTempByteSize);
                                //지금 읽은 메시지 복사
                                Array.Copy(buffer, 0, pocessBuffer, nTempByteSize, messageLength);
                            }
                            else
                            {
                                //남았던 메시지가 없으면 지금 읽어온 메시지를 저장
                                Array.Copy(buffer, 0, pocessBuffer, 0, messageLength);
                            }

                            //처리해야 하는 메시지의 길이가 0이 아니라면
                            if (nTempByteSize + messageLength > 0)
                            {
                                //받은 메시지 처리
                                OnIncomingData(client, pocessBuffer);
                            }
                        }
                    }
                }

                //연결된 클라이언트 목록(connectedClients)에 추가가 되어 foreach문을 타게 되지만 내용은 안들어가서 client가 null이 되는 현상이 발생하여 딜레이를 준다
                Thread.Sleep(10);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    /// <summary>
    /// 클라이언트 접속 확인
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    private bool IsConnected(TcpClient client)
    {
        try
        {
            if(client != null && client.Client != null && client.Client.Connected)
            {
                if(client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 받은 메시지 처리
    /// </summary>
    /// <param name="client"></param>
    /// <param name="data"></param>
    private void OnIncomingData(ServerClient client, byte[] data)
    {
        // 데이터의 크기가 헤더의 크기보다도 작으면
        if (data.Length < Constants.HEADER_SIZE)
        {
            Array.Copy(data, 0, tempBuffer, nTempByteSize, data.Length);     // 임지 저장 버퍼에 지금 메시지 저장
            isTempByte = true;
            nTempByteSize += data.Length;
            return;
        }

        //헤더부분 잘라내기(복사하기)
        byte[] headerDataByte = new byte[Constants.HEADER_SIZE]; //헤더 사이즈는 6(ID:ushort + Size:int)
        Array.Copy(data, 0, headerDataByte, 0, headerDataByte.Length); //헤더 사이즈 만큼 데이터 복사
        //헤더 데이터 구조체화(마샬링)
        stHeader headerData = HeaderfromByte(headerDataByte);

        // 헤더의 사이즈보다 남은 메시지의 사이즈가 작으면
        if (headerData.PacketSize > data.Length)
        {
            Array.Copy(data, 0, tempBuffer, nTempByteSize, data.Length);     // 임지 저장 버퍼에 지금 메시지 저장
            isTempByte = true;
            nTempByteSize += data.Length;
            return;
        }

        //헤더나머지부분 잘라내기(복사하기)
        byte[] msgData = new byte[headerData.PacketSize]; //패킷 분리를 위한 현재 읽은 헤더의 패킷 사이즈만큼 버퍼 생성
        Array.Copy(data, 0, msgData, 0, headerData.PacketSize); //생성된 버퍼에 패킷 정보 복사

        //헤더의 메시지가
        if (headerData.MsgID == 0)//내 정보 확인
        {
            //클라이언트의 정보를 클라이언트에게 보낸다.
            Send("이름 : " + client.clientname + "\nX : " + client.quaternion.x + "\nY : " + client.quaternion.y + "\nZ : " + client.quaternion.z);
        }
        else if(headerData.MsgID == 1)//내 정보 변경
        {
            stChangeInfoMsg stChangeInfoMsg1 = ChangeInfoMsgfromByte(msgData);

            receive_changeInfo_MSG.Enqueue(stChangeInfoMsg1);
        }
        else if (headerData.MsgID == 2)//메시지
        {
            stSendMessageMsg SendMessageMsg = SendMessageMsgfromByte(msgData);
            //메시지 로그에 기록
            logList.Add(client.clientname + " : " + SendMessageMsg.strSendMessage);
        }
        else//식별되지 않은 ID
        {

        }

        // 모든 메시지가 처리되서 남은 메시지가 없을 경우 
        if (data.Length == msgData.Length)
        {
            isTempByte = false;
            nTempByteSize = 0;
        }
        // 메시지 처리 후 메시지가 남아있는 경우
        else
        {
            //임시 버퍼 청소
            Array.Clear(tempBuffer, 0, tempBuffer.Length);

            //생성된 버퍼에 패킷 정보 복사
            Array.Copy(data, msgData.Length, tempBuffer, 0, data.Length - (msgData.Length));// 임시 저장 버퍼에 남은 메시지 저장
            isTempByte = true;
            nTempByteSize += data.Length;
        }
    }

    /// <summary>
    /// 로그 전시
    /// </summary>
    /// <param name="message"></param>
    public void WriteLog(/*Time*/string message)
    {
        ServerLog.GetComponent<Text>().text += message + "\n";
    }

    /// <summary>
    /// 클라이언트 정보 갱신
    /// </summary>
    public void UpdatedClientInfo()
    {
        //클라이언트 정보를 찾아서 갱신
        GameObject.Find("ClientInformation").GetComponent<Text>().text =
        "클라이언트 정보\n" +
        "Name : " + client.clientname + "\n" +
        "Quaternion_X : " + client.quaternion.x.ToString() + "\n" +
        "Quaternion_Y : " + client.quaternion.y.ToString() + "\n" +
        "Quaternion_Z : " + client.quaternion.z.ToString();
    }

    /// <summary>
    /// 메시지 전송
    /// </summary>
    public void Send(string message = "")
    {
        //서버가 연상태가 아니라면
        if (!serverReady)
            return;

        //공지가 아닌경우 입력한 텍스트 전송
        if(message == "")
        {
            message = Text_Input.text;
        }

        try
        {
            //정보 변경 구조체 초기화
            stSendMessageMsg stSendMessageMsgData = new stSendMessageMsg();

            //메시지 작성
            stSendMessageMsgData.MsgID = 0;//메시지 id
            stSendMessageMsgData.PacketSize = (ushort)Marshal.SizeOf(stSendMessageMsgData);//메시지 크기
            stSendMessageMsgData.strSendMessage = message;

            //구조체 메시지 바이트화 및 전송
            byte[] sendMessageByte = GetSendMessageMsgToByte(stSendMessageMsgData);

            //전송
            stream.Write(sendMessageByte, 0, sendMessageByte.Length);
            stream.Flush();

            //로그 기록
            logList.Add("서버 : " + message);
        }
        catch (Exception e)
        {
            Debug.Log("SendException " + e.ToString());
        }
    }   

    /// <summary>
    /// 서버 닫기
    /// </summary>
    public void CloseSocket()
    {
        //서버를 연적이 없다면
        if (!serverReady)
        {
            return;
        }
        else//초기화
        {
            //클라이언트에게 서버 종료 선언
            Send("서버 종료!");

            //stream 초기화
            stream.Close();

            //소켓 종료 및 초기화
            if (tcpListener != null) { tcpListener.Stop(); tcpListener = null; }

            //상태 초기화
            serverReady = false;

            //쓰레드 초기화
            tcpListenerThread.Abort();
            tcpListenerThread = null;

            //연결된 클라이언트 초기화
            client = null;
        }
    }

    /// <summary>
    /// 어플 종료시
    /// </summary>
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
}
