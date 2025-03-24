using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using System.Runtime.InteropServices;//마샬링을 위한 어셈블리

public class NetworkManager_Client : MonoBehaviour
{
    //쓰레드
    private Thread tcpListenerThread;

    //소켓
    private TcpClient socketConnection;
    private NetworkStream stream;

    //상태
    private bool clientReady;

    //ip, port
    public string ip;
    public int port;

    //로그
    public Text ClientLog;
    private List<string> logList;
    //전송 메시지
    public GameObject Text_Input;

    //서버 기능 UI
    public GameObject ButtonServerOpen;
    public GameObject ButtonServerClose;
    public GameObject ServerFunctionUI;

    //받은 데이터 저장공간
    byte[] buffer;
    //받은 데이터가 잘릴 경우를 대비하여 임시버퍼에 저장하여 관리
    byte[] tempBuffer;//임시버퍼
    bool isTempByte;//임시버퍼 유무
    int nTempByteSize;//임시버퍼의 크기

    //보내는 메시지 저장공간
    byte[] sendMessage = new byte[1024];

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

        //받은 데이터 저장공간 초기화
        buffer = new byte[1024];
        //임시버퍼 초기화
        tempBuffer = new byte[1024];
        isTempByte = false;
        nTempByteSize = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //로그리스트에 쌓였다면
        if (logList.Count > 0)
        {
            //배출
            WriteLog(logList[0]);
            logList.RemoveAt(0);
        }

        //클라이언트 상태에 따라 서버 버튼 활성화/비활성화
        ButtonServerOpen.SetActive(!clientReady);
        ButtonServerClose.SetActive(!clientReady);
        ServerFunctionUI.SetActive(!clientReady);
    }

    /// <summary>
    /// 서버 연결
    /// </summary>
    public void ConnectToTcpServer()
    {
        //ip, port 설정
        ip = GameObject.Find("Text_IP").GetComponent<InputField>().text;
        port = int.Parse(GameObject.Find("Text_Port").GetComponent<InputField>().text);

        // TCP클라이언트 스레드 시작
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequeset));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    /// <summary>
    /// TCP클라이언트 쓰레드
    /// </summary>
    private void ListenForIncommingRequeset()
    {
        try
        {
            //연결
            socketConnection = new TcpClient(ip, port);
            stream = socketConnection.GetStream();
            clientReady = true;

            //로그 기록
            logList.Add("시스템 : 서버 연결(ip:" + ip + "/port:" + port + ")");

            //데이터 리시브 항시 대기
            while (true)
            {
                //연결 끊김 감지
                if (!IsConnected(socketConnection))
                {
                    //연결 해제
                    DisConnect();
                    break;
                }

                //연결 중
                if (clientReady)
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
                            OnIncomingData(pocessBuffer);
                        }
                    }
                }
                else//socketReady == false
                {
                    //연결 해제시
                    break;
                }
            }
        }
        catch (SocketException socketException)
        {
            //로그 기록
            logList.Add("시스템 : 서버 연결 실패(ip:" + ip + "/port:" + port + ")");
            logList.Add(socketException.ToString());

            //클라이언트 연결 실패
            clientReady = false;
        }
    }

    /// <summary>
    /// 접속 확인
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    private bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
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
    /// <param name="data"></param>
    private void OnIncomingData(byte[] data)
    {
        //헤더부분 잘라내기(복사하기)
        byte[] headerDataByte = new byte[Constants.HEADER_SIZE];// 헤더 사이즈는 6(ID:ushort + Size:int)
        Array.Copy(data, 0, headerDataByte, 0, headerDataByte.Length);// 헤더 사이즈 만큼 데이터 복사
        //헤더 데이터 구조체화(마샬링)
        stHeader headerData = HeaderfromByte(headerDataByte);

        //헤더나머지부분 잘라내기(복사하기)
        byte[] msgData = new byte[headerData.PacketSize];// 패킷 분리를 위한 현재 읽은 헤더의 패킷 사이즈만큼 버퍼 생성
        Array.Copy(data, 0, msgData, 0, headerData.PacketSize);// 생성된 버퍼에 패킷 정보 복사

        //헤더의 메시지가
        if (headerData.MsgID == 0)//메시지 //id 2로 변경
        {
            stSendMessageMsg SendMessageMsg = SendMessageMsgfromByte(msgData);

            //메시지 로그에 기록
            logList.Add("서버 : " + SendMessageMsg.strSendMessage);
        }
        else//식별되지 않은 ID
        {

        }
    }

    /// <summary>
    /// 메시지 전송
    /// </summary>
    public void Send()
    {
        //연결상태가 아닌 경우
        if(socketConnection == null)
        {
            return;
        }

        //정보 변경 구조체 초기화
        stSendMessageMsg stSendMessageMsgData = new stSendMessageMsg();

        string strSendMessage = Text_Input.GetComponent<InputField>().text;

        //메시지 작성
        stSendMessageMsgData.MsgID = 2;//메시지 id
        stSendMessageMsgData.PacketSize = (ushort)Marshal.SizeOf(stSendMessageMsgData);//메시지 크기
        stSendMessageMsgData.strSendMessage = strSendMessage;

        //구조체 메시지 바이트화 및 전송
        SendMsg(GetSendMessageMsgToByte(stSendMessageMsgData));

        //로그 기록
        logList.Add("나 : " + strSendMessage);
    }

    /// <summary>
    /// 로그 전시
    /// </summary>
    /// <param name="message"></param>
    public void WriteLog(/*Time*/string message)
    {
        ClientLog.GetComponent<Text>().text += message + "\n";
    }

    /// <summary>
    /// 연결 해제
    /// </summary>
    public void DisConnect()
    {
        //미 연결시
        if(socketConnection == null)
        {
            return;
        }

        //로그 기록
        logList.Add("클라이언트 : 연결 해제");

        //상태 초기화
        clientReady = false;

        //stream 초기화
        stream.Close();

        //소켓 초기화
        socketConnection.Close();
        socketConnection = null;

        //쓰레드 초기화
        tcpListenerThread.Abort();
        tcpListenerThread = null;
    }

    /// <summary>
    /// 어플 종료시
    /// </summary>
    private void OnApplicationQuit()
    {
        DisConnect();
    }

    /// <summary>
    /// 내 정보 확인
    /// </summary>
    public void CheckMyInformation()
    {
        //메시지 초기화
        sendMessage = new byte[1024];

        //내 정보 확인 구조체 초기화
        stHeader stCheckInfoMsgData = new stHeader();

        stCheckInfoMsgData.MsgID = 0;
        stCheckInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stCheckInfoMsgData);

        //내 정보 확인 요청 메시지ID 전송
        SendMsg(GetHeaderToByte(stCheckInfoMsgData));
    }

    /// <summary>
    /// 내 정보 변경
    /// </summary>
    public void ChangeMyInformation()
    {
        //메시지 초기화
        sendMessage = new byte[1024];

        stChangeInfoMsg stChangeInfoMsgData = new stChangeInfoMsg();

        //변경할 내 정보 가져오기
        string name = GameObject.Find("Text_Name").GetComponent<InputField>().text;
        byte[] nameByte = Encoding.Default.GetBytes(name);//String -> Byte
        float quaternionX = float.Parse(GameObject.Find("Text_Quaternion_X").GetComponent<InputField>().text);//정보 이름 변경
        float quaternionY = float.Parse(GameObject.Find("Text_Quaternion_Y").GetComponent<InputField>().text);
        float quaternionZ = float.Parse(GameObject.Find("Text_Quaternion_Z").GetComponent<InputField>().text);
        float[] quaArr = {quaternionX, quaternionY, quaternionZ};

        //메시지 작성
        stChangeInfoMsgData.MsgID = 1;
        stChangeInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stChangeInfoMsgData);
        stChangeInfoMsgData.strClientName = name;
        stChangeInfoMsgData.quaternion = quaArr;

        //보내기
        SendMsg(GetChangeInfoMsgToByte(stChangeInfoMsgData));
    }

    /// <summary>
    /// 매개변수 메시지 보내기
    /// </summary>
    private void SendMsg(byte[] message)
    {
        //연결상태가 아닌 경우
        if (socketConnection == null)
        {
            return;
        }

        //전송
        stream.Write(message,0, message.Length);
        stream.Flush();
    }
}
