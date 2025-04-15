using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class StateObject
{
    public Socket workSocket = null;
    public const int bufferSize = 1024;
    public byte[] receiveBuffer = new byte[bufferSize];
}
public class TCPSocketManager : MonoBehaviour
{
    #region type def
    public enum StringType
    {
        LengthType_8,
        LengthType_16,
        LengthType_32,
    }

    public enum ResultType
    {
        SUCCESS = 1,
        FAIL = 2,
    }

    private enum ClanJoinResultType
    {
        Approval = 1,
        Refusal,
        Registered,
        ClanClassError,
        MemberOver,
        LogOut,
        JoinRequesterOver,
        JoinRequesterReject,
        AlreadyJoinRequest,
    }

    private enum ClanManageResultType
    {
        Success = 1,
        Fail_ClanClassError,
        Fail_Erorr,
    }

    #endregion // type def




    #region serialized fields
    [SerializeField] private float KEEP_ALIVE_TERM = 10f;
    [SerializeField] private float KEEP_ALIVE_CHECK = 3f;

    #endregion

    #region private variables

    private static TCPSocketManager _instance = null;
    private string _ip; // = "192.168.0.150";
    private int _port; // = 19001;
    private int _userIndex;
    private bool[] _isChangeClanSettings = new bool[7];
    private bool _isClanNameOverlap = false;

    #endregion






    #region properties

    public static TCPSocketManager Instance { get { return _instance; } }

    #endregion






    #region mono funcs

    private void Awake() {
        _instance = this;
    }

    private void Start() {
        SystemManager.Instance.MakeDontDestroy(this.gameObject);

#if SHOW_LOG
        Debug.Log("TCPSocketManager : Start!\n");
#endif // SHOW_LOG
    }

    //void OnDestroy() {
    //    DisConnect();
    //}

    #endregion








    #region packet

    enum Type
    {
        /// <summary>
        /// 로그인 실패
        /// </summary>
        LR_FAIL = 0,
        /// <summary>
        /// 로그인 성공
        /// </summary>
        LR_SUCCESS = 1,
        /// <summary>
        /// 중복 로그인
        /// </summary>
        LR_FAIL_REPITITION = 2,
        /// <summary>
        /// invalid useridx
        /// </summary>
        LR_FAIL_INDEX = 3,
        /// <summary>
        /// invalid userid
        /// </summary>
        LR_FAIL_ID = 4,
        /// <summary>
        /// invalid password
        /// </summary>
        LR_FAIL_PASSWORD = 5,
        /// <summary>
        /// blocked userid
        /// </summary>
        LR_FAIL_BLOCKED = 6,
        /// <summary>
        /// DB 오류
        /// </summary>
        LR_FAIL_DB = 7,
        /// <summary>
        /// IP 규정 위반
        /// </summary>
        LR_FAIL_IP = 8,
        /// <summary>
        /// 계정 기간 만료
        /// </summary>
        LR_FAIL_EXPIRATION = 9,
        /// <summary>
        /// 사용 시간 때 위반
        /// </summary>
        LR_FAIL_USETIME = 10,
        /// <summary>
        /// 버전 위반
        /// </summary>
        LR_FAIL_VERSION = 11,
        /// <summary>
        /// 이메일 없음
        /// </summary>
        LR_FAIL_EMAIL = 12,
        /// <summary>
        /// 로그인 실패 (가입되지 않은 ID or ID나 PW가 틀림)
        /// </summary>
        LR_FAIL_ID_OR_PW = 13,
        /// <summary>
        /// 핵 툴 사용으로 MaxUser가 넘어도 입장되는 자 막음
        /// </summary>
        LR_FAIL_OVERMAXUSER = 14,

        /// <summary>
        /// 게임 서버 로그인 요청
        /// </summary>
        CS_GAMELOGIN = 106,
        /// <summary>
        /// 게임 서버 로그인 응답
        /// </summary>
        SC_GAMELOGIN = 107,

        /// <summary>
        /// 캐릭터 정보 요청??
        /// </summary>
        CS_CLAN_ON = 400,

        /// <summary>
        /// 캐릭터 정보 응답
        /// </summary>
        SC_CLAN_ON = 401,

        ///<summary>
        ///길드원 정보 요청
        ///</summary>
        CS_CLAN_MEMBERLIST = 407,

        ///<summary>
        ///길드원 정보 응답
        ///</summary>
        SC_CLAN_MEMBERLIST = 408,

        ///<summary>
        ///길드 생성 요청
        ///</summary>
        CS_CLAN_CREATE = 409,

        ///<summary>
        ///길드 생성 응답
        ///</summary>
        SC_CLAN_CREATE = 410,

        /// <summary>
        /// 길드 가입 요청
        /// </summary> 
        CS_CLAN_JOIN = 411,

        /// <summary>
        /// 길드 가입 요청 결과값
        /// </summary> 
        SC_CLAN_JOIN = 412,

        /// <summary>
        /// 초대 받은 유저 길드 초대 알림
        /// </summary> 
        SC_REQUEST_JOINCLAN = 413,

        /// <summary>
        /// 길드 가입 승인
        /// </summary> 
        CS_CLAN_APPROVAL = 414,

        /// <summary>
        /// 길드 가입 승인 결과
        /// </summary> 
        SC_CLAN_APPROVAL = 415,

        /// <summary>
        /// 접속한 길드원들에게 보냄
        /// </summary> 
        SC_BROADCAST_CLAN_APPROVAL = 416,

        /// <summary>
        /// 길드 거절 요청
        /// </summary>
        CS_CLAN_REFUSAL = 417,

        /// <summary>
        /// 길드 거절 요청 결과값
        /// </summary>
        SC_CLAN_REFUSAL = 418,

        /// <summary>
        /// 길드 탈퇴 요청
        /// </summary>
        CS_CLAN_KICKOUT = 419,

        /// <summary>
        /// 길드 탈퇴 알림
        /// </summary>
        SC_CLAN_KICKOUT = 420,

        /// <summary>
        /// 클랜 해제 요청
        /// </summary>
        CS_CLAN_DISSOLVE = 421,

        /// <summary>
        /// 클랜 해제 결과
        /// </summary>
        SC_CLAN_DISSOLVE = 422,

        /// <summary>
        /// 클랜원 등급 변경 요청
        /// </summary>
        CS_CLAN_CHANGE_CLASS = 423,

        /// <summary>
        /// 클랜원 등급 변경 결과
        /// </summary>
        SC_CLAN_CHANGE_CLASS = 424,

        /// <summary>
        /// 클랜 소개글 변경 요청
        /// </summary>
        CS_CLAN_CHANGE_INTRO = 425,

        /// <summary>
        /// 클랜 소개글 변경 결과
        /// </summary>
        SC_CLAN_CHANGE_INTRO = 426,

        /// <summary>
        /// 클랜 네임 변경
        /// </summary>
        CS_CLAN_CHANGE_CLANNAME = 427,

        /// <summary>
        /// 클랜 네임 결과
        /// </summary>
        SC_CLAN_CHANGE_CLANNAME = 428,

        /// <summary>
        /// 클랜 깃발 변경 요청
        /// </summary>
        CS_CLAN_CHANGE_FLAG = 429,

        /// <summary>
        /// 클랜 깃발 변경 결과
        /// </summary>
        SC_CLAN_CHANGE_FLAG = 430,

        /// <summary>
        /// 클랜 유형 변경
        /// </summary>
        CS_CLAN_CHANGE_TYPE = 431,

        /// <summary>
        /// 클랜 유형 변경 결과
        /// </summary>
        SC_CLAN_CHANGE_TYPE = 432,

        /// <summary>
        /// 클랜 승점 변경 요청
        /// </summary>
        CS_CLAN_CHANGE_VICTORYPOINT = 433,

        /// <summary>
        /// 클랜 승점 변경 결과
        /// </summary>
        SC_CLAN_CHANGE_VICTORYPOINT = 434,

        /// <summary>
        /// 클랜 언어 변경 요청
        /// </summary>
        CS_CLAN_CHANGE_LANGUAGE = 435,

        /// <summary>
        /// 클랜 언어 변경 결과
        /// </summary>
        SC_CLAN_CHANGE_LANGUAGE = 436,

        ///<summary>
        ///길드원들에게 ON/OFF 상태 알림
        ///</summary>
        SC_NOTIFY_LOGOUT = 442,

        ///<summary>
        ///홍보글 변경 요청
        ///</summary>
        CS_CLAN_CHANGE_PROMOTION = 446,

        ///<summary>
        ///홍보글 변경 결과
        ///</summary>
        SC_CLAN_CHANGE_PROMOTION = 447,

        /// <summary>
        /// 길드 채팅
        /// </summary>
        CS_CLAN_CHAT = 437,

        /// <summary>
        /// 클랜전 참가 여부 변경
        /// </summary>
        CS_CLAN_CHANGE_ATTENDSTATUS = 448,

        /// <summary>
        /// 클랜전 참가 여부 변경
        /// </summary>
        SC_CLAN_CHANGE_ATTENDSTATUS = 449,

        /// <summary>
        /// 일반 채팅
        /// </summary>
        CS_CHAT = 439,

        /// <summary>
        /// 일반채팅
        /// </summary>
        SC_CHAT = 440,

        /// <summary>
        /// 클랜 알림
        /// </summary>
        SC_CLAN_NOTIFY = 443,

        /// <summary>
        /// 마스터 클랜 변경 및 대표 이전
        /// </summary>
        CS_CLAN_CHANGE_MASTER = 444,

        /// <summary>
        /// 마스터 클랜 변경 및 대표 이전 응답
        /// </summary>
        SC_CLAN_CHANGE_MASTER = 445,

        /// <summary>
        /// 클랜전 맵 입장
        /// </summary>
        CS_CLAN_BATTLE_ON = 460,
        /// <summary>
        /// 클랜전 맵 입장 응답
        /// </summary>
        SC_CLAN_BATTLE_ON = 461,

        /// <summary>
        /// 클랜전 맵 종료
        /// </summary>
        CS_CLAN_BATTLE_OFF = 462,
        /// <summary>
        /// 클랜전 맵 종료 응답
        /// </summary>
        SC_CLAN_BATTLE_OFF = 463,

        /// <summary>
        /// 클랜전 예약 / 공격 여부
        /// </summary>
        CS_CLAN_BATTLE_PROGRESS = 464,
        /// <summary>
        /// 클랜전 예약 / 공격 여부 응답
        /// </summary>
        SC_CLAN_BATTLE_PROGRESS = 465,

        /// <summary>
        /// 클랜 적군 온라인 여부 요청
        /// </summary>
        CS_CLAN_LIVE_MEMBERS = 466,
        /// <summary>
        /// 클랜 적군 온라인 여부 응답
        /// </summary>
        SC_CLAN_LIVE_MEMBERS = 467,

        /// <summary>
        /// 클랜전 전투 결과 요청
        /// </summary>
        CS_CLAN_BATTLE_END = 468,
        /// <summary>
        /// 클랜전 전투 결과 응답
        /// </summary>
        SC_CLAN_BATTLE_END = 469,

        /// <summary>
        /// 클랜전 전부 사망 후 요청
        /// </summary>
        CS_CLAN_BATTLE_REVIVAL = 470,
        /// <summary>
        /// 클랜전 전부 사망 후 응답
        /// </summary>
        SC_CLAN_BATTLE_REVIVAL = 471,

        ///<summary>
        ///길드 탈퇴 요청
        ///</summary>
        CS_REQ_KICKOUT_CLAN = 716,

        ///<summary>
        ///길드 탈퇴 알림
        ///</summary>
        SC_ANS_KICKOUT_CLAN = 717,

        ///<summary>
        ///길드 레벨 변경 요청
        ///</summary>
        SC_REQ_CLASS_CLAN = 720,

        ///<summary>
        ///길드 레벨 변경 결과값
        ///</summary>
        CS_ANS_CLASS_CLAN = 721,

        ///<summary>
        ///길드 소개글 변경 요청
        ///</summary>
        CS_REQ_CLANINTRO = 722,

        ///<summary>
        ///길드 소개글 변경 알림
        ///</summary>
        SC_ANS_CLANINTRO = 723,

        /// <summary>
        /// 중복 로그인일 경우 처리
        /// </summary>
        SC_DUPLICATELOGIN = 108,

        CS_KEEPALIVE = 109,
        SC_KEEPALIVE = 110,
    }



    class Packet
    {
        public List<byte> total = new List<byte>();   // 전체 (버퍼 사이즈 + 타입 + 버퍼)
        public List<byte> buffer = new List<byte>();   // 버퍼 (데이터 사이즈 + 데이터 + ...)

        public ushort packetType;    // 타입

        public Packet(Type type) {
            buffer.Clear();    // 버퍼 초기화

            this.packetType = (ushort)type;
        }

        public void AddString(string stringData) {
            byte[] data = Encoding.UTF8.GetBytes(stringData);

            //buffer.AddRange(BitConverter.GetBytes((ushort)data.Length));  // 패킷에 데이터 사이즈 추가
            buffer.AddRange(data);    // 패킷에 데이터 추가
        }

        /// <summary>
        /// u_int08 값을 패킷에 추가
        /// </summary>
        /// <param name="byteData"></param>
        public void AddUint08(byte byteData) {
            byte[] data = new byte[] { byteData };
            //buffer.AddRange(BitConverter.GetBytes((byte)1));      // 패킷에 데이터 사이즈 추가
            buffer.AddRange(data);
        }

        /// <summary>
        /// u_int16 값을 패킷에 추가
        /// </summary>
        /// <param name="ushortData"></param>
        public void AddUint16(ushort ushortData) {
            byte[] data = BitConverter.GetBytes(ushortData);

            //buffer.AddRange(BitConverter.GetBytes((ushort)2));    // 패킷에 데이터 사이즈 추가
            buffer.AddRange(data);
        }

        /// <summary>
        /// u_int32 값을 패킷에 추가
        /// </summary>
        /// <param name="uintData"></param>
        public void AddUint32(uint uintData) {
            byte[] data = BitConverter.GetBytes(uintData);
            //buffer.AddRange(BitConverter.GetBytes((uint)4));  // 패킷에 데이터 사이즈 추가
            buffer.AddRange(data);
        }

        /// <summary>
        /// float 값을 패킷에 추가
        /// </summary>
        /// <param name="floatData"></param>
        public void AddFloat(float floatData) {
            byte[] data = BitConverter.GetBytes(floatData);
            //buffer.AddRange(BitConverter.GetBytes((uint)4));  // 패킷에 데이터 사이즈 추가
            buffer.AddRange(data);
        }

        /// <summary>
        /// 패킷 전송 형식에 맞게 패킷에 패킷 사이즈, 패킷 타입, 데이터 버퍼 패킹..
        /// </summary>
        public void Pack() {
            total.Clear();    // 패킷 초기화

            total.AddRange(BitConverter.GetBytes((ushort)buffer.ToArray().Length)); // 패킷에 버퍼 사이즈 추가
            total.AddRange(BitConverter.GetBytes(packetType)); // 패킷에 타입 추가
            total.AddRange(buffer.ToArray()); // 패킷에 버퍼 추가
        }
    }

    #endregion //packet

    #region Buffer

    string GetString(StringType sType) {
        int size = 0;
        if (StringType.LengthType_8 == sType) {
            size = (int)GetUInt8();
        }
        else if (StringType.LengthType_16 == sType) {
            size = (int)GetUInt16();
        }
        else if (StringType.LengthType_32 == sType) {
            size = (int)GetUInt32();
        }
        string value = Encoding.Default.GetString(_receiveArray.GetRange(0, size).ToArray());
        _receiveArray.RemoveRange(0, size);

        return value;
    }

    string GetString() {
        ushort size = GetUInt16();
        //Debug.LogFormat("string size : {0}", size);
        string _Data = Encoding.UTF8.GetString(_receiveArray.GetRange(0, size).ToArray());
        _receiveArray.RemoveRange(0, size);

        return _Data;
    }

    byte GetUInt8() {
        byte value = _receiveArray[0];
        _receiveArray.RemoveRange(0, 1);

        return value;
    }

    ushort GetUInt16(bool shouldRemove = true) {
        ushort value = BitConverter.ToUInt16(_receiveArray.GetRange(0, 2).ToArray(), 0);

        if (shouldRemove) {
            _receiveArray.RemoveRange(0, 2);
        }

        return value;
    }

    uint GetUInt32() {
        uint value = BitConverter.ToUInt32(_receiveArray.GetRange(0, 4).ToArray(), 0);
        _receiveArray.RemoveRange(0, 4);

        return value;
    }

    float GetFloat() {
        byte[] bytes = _receiveArray.GetRange(0, 4).ToArray();
        //float value = BitConverter.ToSingle(BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes, 0);
        float value = BitConverter.ToSingle(_receiveArray.GetRange(0, 4).ToArray(), 0);

        _receiveArray.RemoveRange(0, 4);

        return value;
    }

    #endregion Buffer

    #region Socket

    enum SocketState
    {
        None,

        /// <summary>
        /// 게임 서버 연결 시도
        /// </summary>
        TryToConnect,
        /// <summary>
        /// 게임 서버 연결 완료
        /// </summary>
        Connected,
        /// <summary>
        /// 게임 서버 로그인 시도
        /// </summary>
        TryToLogin,
        /// <summary>
        ///  게임 서버 연결 완료
        /// </summary>
        LoggedIn,

        /// <summary>
        /// 예외..
        /// </summary>
        Exception
    }

    public enum ServerType
    {
        None,
        Game,
    }


    private List<Packet> _sendPacketList = new List<Packet>();  // 보낼 패킷 리스트
    private List<byte> _receiveArray = new List<byte>();    // 리시브 데이터 리스트

    private ManualResetEvent _connectDone = new ManualResetEvent(false);
    private ManualResetEvent _sendDone = new ManualResetEvent(false);
    private ManualResetEvent _receiveDone = new ManualResetEvent(false);

    private Socket _socket = null;

    private ServerType _serverType = ServerType.None;
    private SocketState _socketState = SocketState.None;

    private Coroutine socketUpdateCoroutine;

    public Action UpdateUICallback;
    public Action GoToPrevUI;
    public Action GoToNextUI;
    public Action HidePopup;

    private Action _updateRequesterCallback;
    private Action _changeClanMatserCallback;

    private Coroutine keepAliveCoroutine;
    private Coroutine keepAliveCheckCoroutine;





    public bool IsConnected {
        get {
            if (null != _socket && _socket.Connected) {
                return true;
            }
            return false;
        }
    }





    private void StopSocketUpdate() {
        if (null == socketUpdateCoroutine) {
            return;
        }

        StopCoroutine(socketUpdateCoroutine);
        socketUpdateCoroutine = null;
    }

    private void StartSocketUpdate() {
        //SaveGameServerInfo();
        //LoadGameServerInfo();

        StopSocketUpdate();

        socketUpdateCoroutine = StartCoroutine(SocketUpdate());
    }

    public void DisConnect() {
#if SHOW_LOG
        Debug.LogWarningFormat("TCPSocketManager : Disconnect..!!\t\t[ ServerType : {0} / SocketState : {1} ]\n",
            _serverType, _socketState);
#endif // SHOW_LOG

        _serverType = ServerType.None;
        _socketState = SocketState.None;

        ClearCallbacks();

        StopKeepAliveCoroutine();

        if (_socket != null && _socket.Connected) {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        _socket = null;
    }

    private void Connect(ServerType sType) {
        _serverType = sType;
        _socketState = SocketState.TryToConnect;

        _sendPacketList.Clear();
        _receiveArray.Clear();

        new Thread(() => {
            IPHostEntry ioHostEntry = Dns.GetHostEntry(_ip); // Find host name
            AddressFamily adFamily = ioHostEntry.AddressList[0].AddressFamily;
            if (AddressFamily.InterNetworkV6 == adFamily) {
                _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else if (AddressFamily.InterNetwork == adFamily) {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            IPAddress ip = IPAddress.Parse(_ip);
            IPEndPoint ep = new IPEndPoint(ip, _port);

            _socket.BeginConnect(ep, new AsyncCallback(ConnectCallback), _socket);
        }).Start();
    }

    private IEnumerator SocketUpdate() {
        var waitFixUpdate = new WaitForFixedUpdate();
        while (true) {
            UpdateSendAndReceive();
            yield return waitFixUpdate;
        }
    }

    private void UpdateSendAndReceive() {
        // Callback에서 호출 안되는 함수용 플래그 처리
        if (SocketState.Connected == _socketState) {
            GameServerLogin();
        }
        else if (SocketState.Exception == _socketState) {
            _socketState = SocketState.None;
        }

        // Send And Receive
        if (_socket != null && _socket.Connected) {
            // Send packets
            if (_sendPacketList.Count > 0) {
                Packet sendPacket = _sendPacketList[0];
                _socket.BeginSend(sendPacket.total.ToArray(), 0, sendPacket.total.ToArray().Length, SocketFlags.None, new AsyncCallback(SendCallback), _socket);

#if SHOW_LOG
                if (Type.CS_KEEPALIVE != (Type)sendPacket.packetType) {
                    Debug.LogFormat("TCP Send : {0} / SendSize : {1}\n", (Type)sendPacket.packetType, sendPacket.buffer.Count);
                }
#endif // SHOW_LOG

                _sendPacketList.Remove(sendPacket);
            }

            // Receive packets
            lock (_receiveArray) {
                if (_receiveArray.Count >= 2) {                     // 리시브 데이터 길이가 2보다 클때
                    if (_receiveArray.Count >= GetUInt16(false)) {  // 리시브 데이터 길이가 데이터 사이즈보다 클때
                        GetUInt16();                                // 데이터 사이즈 삭제
                        ProcReceivePackets((Type)GetUInt16());      // 받은 패킷 타입별 처리
                    }
                }
            }
        }
    }

    private void ConnectCallback(IAsyncResult _AR) {
        try {
            Socket client = (Socket)_AR.AsyncState;
            client.EndConnect(_AR);
            _connectDone.Set();

            StateObject stateObject = new StateObject {
                workSocket = _socket,
            };

            client.BeginReceive(stateObject.receiveBuffer, 0, StateObject.bufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), stateObject);

            _socketState = SocketState.Connected;

#if SHOW_LOG
            Debug.Log("TCP : Connect Success!");
#endif // SHOW_LOG
        }
        catch (Exception e) {
            Debug.LogAssertionFormat("TCP : Connect Exception : {0}", e);
            _socketState = SocketState.Exception;
        }
    }

    private void SendCallback(IAsyncResult asyncResult) {
        Socket client = (Socket)asyncResult.AsyncState;
        client.EndSend(asyncResult);
        _sendDone.Set();
    }

    private void ReceiveCallback(IAsyncResult asyncResult) {
        StateObject stateObject = (StateObject)asyncResult.AsyncState;
        Socket client = stateObject.workSocket;
        if (client == null) {
            return;
        }

        if (!client.Connected) {
            Debug.LogError("TCPSocketManager : ReceiveCallback : Not connected..!!\n");
            return;
        }

        int readLength = client.EndReceive(asyncResult);

        lock (_receiveArray) {
            _receiveArray.AddRange(stateObject.receiveBuffer.Take(readLength)); // 리시브 리스트에 받은 데이터 추가
            _receiveDone.Set();
        }

        client.BeginReceive(stateObject.receiveBuffer, 0, StateObject.bufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), stateObject);
    }

    #endregion Socket






    #region public functions

    public void Connect(string ip, int port, int userIndex) {
        _ip = ip;
        _port = port;
        _userIndex = userIndex;

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : Connect : Set tcp server info.. [ ip : {0}\t port : {1}\t userIndex : {2}]", _ip, _port, _userIndex);
        Debug.Log("TCP : Try to Connect to Server..!!\n");
#endif

        Connect(ServerType.Game);

        StartSocketUpdate();
    }

    public void ClearCallbacks() {
        UpdateUICallback = null;
        GoToPrevUI = null;
        GoToNextUI = null;
    }

    public void RequestGameLogin() {

    }

    public bool IsLoggedInGameServer() {
        return SocketState.LoggedIn == _socketState;
    }

    public void RequestClanON() {
        SendSimplePacket(Type.CS_CLAN_ON);
    }

    public void RequestClanCreate() {
        SendSimplePacket(Type.CS_CLAN_CREATE);
    }

    public void RequestClanJoin(int clanID, string joinMessage = "") {
        Packet packet = new Packet(Type.CS_CLAN_JOIN);
        PlayerData playerData = PlayerManager.Instance.GetPlayerData();
        int avataID = 0;
        int bgId = 0;
        int pinId = 0;
        int level = playerData._level;
        string nickName = playerData._nickName;
        int nickName_lan = Encoding.UTF8.GetByteCount(nickName);
        string message = joinMessage;
        int message_lan = Encoding.UTF8.GetByteCount(message);

        packet.AddUint32((uint)clanID);
        packet.AddUint16((ushort)avataID);
        packet.AddUint16((ushort)bgId);
        packet.AddUint16((ushort)pinId);
        packet.AddUint16((ushort)level);
        packet.AddUint16((ushort)nickName_lan);
        packet.AddString(nickName);
        packet.AddUint16((ushort)message_lan);
        if (message != string.Empty) {
            packet.AddString(message);
        }
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanKickOut(Action Requestercallback) {
        Packet packet = new Packet(Type.CS_CLAN_KICKOUT);
        int userSeq = PlayerManager.Instance.UserSeq;

        packet.AddUint32((uint)userSeq);
        packet.Pack();

        _sendPacketList.Add(packet);

        _updateRequesterCallback = Requestercallback;
    }

    public void RequestChangeClanSettingName(string clanName) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_CLANNAME);
        int clanNameSize = Encoding.UTF8.GetByteCount(clanName);

        packet.AddUint16((ushort)clanNameSize);
        packet.AddString(clanName);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[0] = true;
    }

    public void RequestChangeClanSettingDesc(string clanDesc) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_INTRO);
        int clanDescSize = Encoding.UTF8.GetByteCount(clanDesc);

        packet.AddUint16((ushort)clanDescSize);
        packet.AddString(clanDesc);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[1] = true;
    }

    public void RequestChangeClanSettingJoinType(int joinType) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_TYPE);

        packet.AddUint08((byte)joinType);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[2] = true;
    }

    public void RequestChangeClanSettingFlag(int patternIndex, int symbolIndex) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_FLAG);

        packet.AddUint16((ushort)patternIndex);
        packet.AddUint16((ushort)symbolIndex);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[3] = true;
    }

    public void RequestChangeClanSettingVictoryPoint(int winscore) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_VICTORYPOINT);

        packet.AddUint32((uint)winscore);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[4] = true;
    }

    public void RequestChangeClanSettingLanguage(int languageIndex) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_LANGUAGE);

        packet.AddUint08((byte)languageIndex);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[5] = true;
    }

    public void RequestChangeClanSettingNotice(string clanNotice) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_PROMOTION);

        int clanNoticeSize = Encoding.UTF8.GetByteCount(clanNotice);

        packet.AddUint16((ushort)clanNoticeSize);
        packet.AddString(clanNotice);
        packet.Pack();

        _sendPacketList.Add(packet);

        _isChangeClanSettings[6] = true;
    }

    public void RequestChangeClanMemberClass(int targetUserSeq, int clanLevel, int changeClass, Action requesterCallback) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_CLASS);

        packet.AddUint32((uint)targetUserSeq);
        packet.AddUint08((byte)clanLevel);
        packet.AddUint08((byte)changeClass);
        packet.Pack();

        _sendPacketList.Add(packet);

        GoToPrevUI = requesterCallback;
    }

    public void RequestClanDissolve() {
        SendSimplePacket(Type.CS_CLAN_DISSOLVE);
    }

    public void RequestChangeClanMaster(int targetUserSeq, Action UpdateUI, Action changeClanMatserCallback) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_MASTER);

        packet.AddUint32((uint)targetUserSeq);
        packet.Pack();

        _sendPacketList.Add(packet);

        UpdateUICallback = UpdateUI;
        _changeClanMatserCallback = changeClanMatserCallback;
    }

    public void RequestClanLoginMemberList() {
        SendSimplePacket(Type.CS_CLAN_MEMBERLIST);
    }

    public void RequestClanChat(int clanID, int chatType, string chat) {
        int chatSize = Encoding.UTF8.GetByteCount(chat);

        Packet packet = new Packet(Type.CS_CLAN_CHAT);

        packet.AddUint32((uint)clanID);
        packet.AddUint16((ushort)chatType);
        packet.AddUint16((ushort)chatSize);
        packet.AddString(chat);
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanRefusal(int joinRequestUserSeq) {
        Packet packet = new Packet(Type.CS_CLAN_REFUSAL);

        packet.AddUint32((uint)joinRequestUserSeq);
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanApproval(int joinRequestUserSeq) {
        Packet packet = new Packet(Type.CS_CLAN_APPROVAL);

        packet.AddUint32((uint)joinRequestUserSeq);
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestChangeClanSettingWar(int clanWarAttend) {
        Packet packet = new Packet(Type.CS_CLAN_CHANGE_ATTENDSTATUS);

        packet.AddUint08((byte)clanWarAttend);
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanWarMapEnter() {
        Packet packet = new Packet(Type.CS_CLAN_BATTLE_ON);

        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanWarMapClose() {
        Packet packet = new Packet(Type.CS_CLAN_BATTLE_OFF);

        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanWarProgress(int vsClanID, int vsUserseq, int progress = 0) {
        Packet packet = new Packet(Type.CS_CLAN_BATTLE_PROGRESS);

        packet.AddUint32((uint)vsClanID);
        packet.AddUint32((uint)vsUserseq);
        packet.AddUint08((byte)progress);
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanWarEnemyOnline(int clanID) {
        Packet packet = new Packet(Type.CS_CLAN_LIVE_MEMBERS);

        packet.AddUint32((uint)clanID);
        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanWarBattleEnd(int point) {
        Packet packet = new Packet(Type.CS_CLAN_BATTLE_END);

        packet.AddUint32((uint)GameDataManager.Instance.ClanWarInfo.vsClanInfo.m_clanId);
        packet.AddUint32((uint)GameDataManager.Instance.ClanWarVsUserData.m_userseq);

        List<MDeckCard> clanWarEndDeck = GameDataManager.Instance.GetClanWarEndDeckInfo();
        for (int i = 0, count = Common.teamMemberMax; i < count; ++i) {
            packet.AddUint32((uint)clanWarEndDeck[i].m_cardId);
            packet.AddUint32((uint)clanWarEndDeck[i].m_cardHp);
        }

        packet.AddUint32((uint)PlayerManager.Instance.UserSeq);
        packet.AddUint32((uint)point);

        packet.Pack();

        _sendPacketList.Add(packet);
    }

    public void RequestClanWarRevival(int clanID) {
#if SHOW_LOG
        Debug.LogWarning("TCPSocketManager : RequestClanWarRevival");
#endif // SHOW_LOG

        Packet packet = new Packet(Type.CS_CLAN_BATTLE_REVIVAL);

        packet.AddUint32((uint)clanID);

        packet.Pack();

        _sendPacketList.Add(packet);
    }

    #endregion // public functions






    #region private functions

    private void RefreshClanCommonUI() { // 한 번만 Refresh 하기 위한 함수
        for (int i = 0, count = _isChangeClanSettings.Length; i < count; ++i) {
            if (_isChangeClanSettings[i] == true) {
                _isChangeClanSettings[i] = false;
                break;
            }
        }

        for (int i = 0, count = _isChangeClanSettings.Length; i < count; ++i) {
            if (_isChangeClanSettings[i] == true) {
                return;
            }
        }

        RefreshClanMemberTapUI();

        if (_isClanNameOverlap) {
            _isClanNameOverlap = false;
#if SHOW_LOG
            Debug.Log("TCPSocketManager : RefreshClanCommonUI : ClanName OverLap.!!");
#endif
            return;
        }

        if (HidePopup != null) {
            HidePopup();
            HidePopup = null;
        }
    }

    private void RefreshClanMemberTapUI() {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Clan) {
                ClanUIController clanUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
                ClanTapType clanTaptype = clanUI.TapType;

                //if (clanTaptype == ClanTapType.Member) {
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
                //}
            }
        }
    }

    private void UpdateClanMemberListItem(int userSeq) {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Clan) {
                ClanUIController clanUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
                ClanTapType clanTaptype = clanUI.TapType;
                ClanMemberUI clanMemberUI = clanUI.ClanMemberUI;

                if (clanTaptype == ClanTapType.Member) {
                    clanMemberUI.UpdateMemberListItem(userSeq);
                }
            }
        }
    }

    private void UpdateClanChattingListItem() {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Chatting) {
                ChattingUI chattingUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Chatting) as ChattingUI;
                chattingUI.UpdateChattingListItem();
            }
        }
    }

    private void UpdateClanMiniChattingListItem() {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Clan) {
                ClanUIController clanUIController = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
                clanUIController.UpdateMiniChattingList();
            }
            else if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ClanWarMap) {
                ClanWarMapUI clanWarMapUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ClanWarMap) as ClanWarMapUI;
                clanWarMapUI.UpdateMiniChattingList();
            }
        }
    }

    private void UpdateClanJoinRequestList() {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Clan) {
                ClanUIController clanUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
                ClanTapType clanTaptype = clanUI.TapType;

                if (clanTaptype == ClanTapType.Member) {
                    ClanMemberUI clanMemberUI = clanUI.ClanMemberUI;
                    clanMemberUI.UpdateJoinRequestList();
                }
            }
        }
    }

    private void RefreshClanWarMap(List<int> attackerUIDs, List<int> defenderUIDs, List<int> progress) {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ClanWarMap) {
                ClanWarMapUI clanMapUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ClanWarMap) as ClanWarMapUI;
                clanMapUI.UpdateOnlineAndProgress(attackerUIDs, defenderUIDs, progress);
            }
        }
    }

    private void UpdateClanWarProgress(int attackerUID, int vsClanID, int vsUserUID, int progress) {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            //if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ClanWarMap) {
            ClanWarMapUI clanMapUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ClanWarMap) as ClanWarMapUI;
            clanMapUI.UpdateAttackInfo(attackerUID, vsClanID, vsUserUID, progress);
            //}
        }
    }

    private void UpdateClanWarHistory(int vsClanID, int vsUserID, List<int> cardList, List<int> hpList, int userID, int point) {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ClanWarMap) {
                GameDataManager.Instance.AddClanWarHistory(vsClanID, vsUserID, userID, point);
                ClanWarMapUI clanMapUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ClanWarMap) as ClanWarMapUI;
                clanMapUI.UpdateMap(vsUserID, cardList, hpList);
                clanMapUI.UpdateHistory();
            }
        }
        else {
            GameDataManager.Instance.UpdateClanWarInfo(vsClanID, vsUserID, cardList, hpList, userID);
        }
    }

    private void ClanWarRevive(int vsClanID) {
        if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
            if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ClanWarMap) {
                ClanWarMapUI clanMapUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ClanWarMap) as ClanWarMapUI;
                clanMapUI.ReviveAllEnemy(vsClanID);
            }
        }
        GameDataManager.Instance.ReviveClanWar(vsClanID);
    }

    #endregion






    #region Send

    private void SendSimplePacket(Type sendPacketType) {
        Packet packet = new Packet(sendPacketType);
        packet.Pack();
        _sendPacketList.Add(packet);
    }

    private void SendKeepAlive() {
        SendSimplePacket(Type.CS_KEEPALIVE);
        keepAliveCheckCoroutine = StartCoroutine(CheckKeepAlive());
    }

    private void GameServerLogin() {
        _socketState = SocketState.TryToLogin;

        Packet packet = new Packet(Type.CS_GAMELOGIN);
        packet.AddUint32((uint)_userIndex);
        packet.Pack();

#if SHOW_LOG
        Debug.LogFormat("<color=yellow>TCPSocketManager : GameServerLogin : [ packetype : {0} / {1} ] [ {2} : {3} ]</color>\n",
            (Type)packet.packetType, (ushort)packet.packetType, name, (ushort)_userIndex);
#endif // SHOW_LOG

        _sendPacketList.Add(packet);
    }

    #endregion // Send






    #region Receive

    private void StopKeepAliveCoroutine() {
        if (null == keepAliveCoroutine)
            return;

        StopCoroutine(keepAliveCoroutine);
        keepAliveCoroutine = null;
    }

    private IEnumerator DelayedKeepAlive() {
        yield return new WaitForSecondsRealtime(KEEP_ALIVE_TERM);
        SendKeepAlive();
    }

    private void ReceiveKeepAlive() {
        //#if SHOW_LOG
        //        Debug.LogFormat("TCPSocketManager : ReceivedGameServerLogin : Type : [ SC_KEEPALIVE ]\n");
        //#endif // SHOW_LOG

        StopKeepAliveCoroutine();
        keepAliveCoroutine = StartCoroutine(DelayedKeepAlive());
        StopCheckKeepAliveCoroutine();
    }

    private void StopCheckKeepAliveCoroutine() {
        if (null == keepAliveCheckCoroutine) {
            return;
        }

        StopCoroutine(keepAliveCheckCoroutine);
        keepAliveCheckCoroutine = null;
    }

    private IEnumerator CheckKeepAlive() {
        yield return new WaitForSecondsRealtime(KEEP_ALIVE_CHECK);
#if SHOW_LOG
        Debug.Log("<color=yellow>TCP : Failed to receive keep alive packet. Try ReConnect.!!</color>\n");
#endif // SHOW_LOG

        DisConnect();
        Connect(_ip, _port, _userIndex);
    }

    private void ReceivedGameServerLogin() {
#if SHOW_LOG
        Debug.Log("<color=yellow>TCP : Process [ SC_GAMELOGIN ] - ReceivedGameServerLogin</color>\n");
#endif // SHOW_LOG

        Type type = (Type)GetUInt8();
#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedGameServerLogin : Type : [ {0} ]\n", type);
#endif // SHOW_LOG

        if (Type.LR_FAIL == type) {
            Debug.LogAssertion("TCPSocketManager : ReceivedGameServerLogin : Type : [ LR_FAIL ]\n");
        }
        else if (Type.LR_SUCCESS == type) {
            SendKeepAlive();    // keep alive 시작..!!
            _socketState = SocketState.LoggedIn;

            RequestClanON();

            int clanID = PlayerManager.Instance.ClanID;
            if (clanID != 0) {
                WebHttp.Instance.RequestClanInfo(clanID, LobbySubType.None);
                WebHttp.Instance.RequestClanChattingList(clanID);
                WebHttp.Instance.RequestClanChatBlockUserList();
                WebHttp.Instance.RequestClanJoinUserList();
                WebHttp.Instance.RequestClanWarInfo(null);
            }
        }
        else if (Type.LR_FAIL_REPITITION == type) {
#if SHOW_LOG
            Debug.LogAssertion("TCPSocketManager : ReceivedGameServerLogin : Type : [ LR_FAIL_REPITITION ]\n");
#endif
        }
    }

    private void ReceivedDuplicateLogin() {
#if SHOW_LOG
        Debug.LogWarning("TCP : Process [ SC_DUPLICATELOGIN ] - ReceivedDuplicateLogin\n");
#endif // SHOW_LOG

        DisConnect();
        PopupManager.Instance.ShowOKPopup(TSystemStrings.Instance.FindString("PATCH_1041"),
            TSystemStrings.Instance.FindString("PATCH_1042"),
            () => {
                SystemManager.Instance.ExitGame();
            },
            0, true
            );
    }

    private void ReceivedClanMemberState() {
        int userSeq = (int)GetUInt32();
        int loginState = GetUInt8();

        List<MClanUserInfo> clanUserInfos = PlayerManager.Instance.ClanData._clanUserInfos;

        for (int i = 0, count = clanUserInfos.Count; i < count; ++i) {
            if (userSeq == clanUserInfos[i].m_userseq) {
                clanUserInfos[i].m_login_status = loginState;
                break;
            }
        }

        UpdateClanMemberListItem(userSeq);

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedClanMemberState : Type : [ SC_NOTIFY_LOGOUT ] userSeq : {0}, loginState : {1}\n", userSeq, (LoginStateType)loginState);
#endif // SHOW_LOG
    }

    private void ReceivedClanJoin() {
        int result = GetUInt8();

        if (result == (int)ClanJoinResultType.Approval) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29032"), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.MemberOver) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29098"), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.JoinRequesterOver) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29215"), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.JoinRequesterReject) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29033"), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.AlreadyJoinRequest) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29217"), PopupNormalIconType.Clan, null);
        }

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedClanJoin : Type : [ SC_CLAN_JOIN ] result : {0}", result);
#endif // SHOW_LOG
    }

    private void ReceivedClanKickOut() {
        int result = GetUInt8();
        int orderUserSeq = (int)GetUInt32();
        int targetUserSeq = (int)GetUInt32();

        List<MClanUserInfo> clanMemberList = PlayerManager.Instance.ClanData._clanUserInfos;
        for (int i = 0, count = clanMemberList.Count; i < count; ++i) {
            if (clanMemberList[i].m_userseq == targetUserSeq) {
                clanMemberList.RemoveAt(i);
                break;
            }
        }

        PlayerManager.Instance.SubtractClanMemberCount();

        if (_updateRequesterCallback != null) {
            _updateRequesterCallback();
            _updateRequesterCallback = null;
        }

        RefreshClanMemberTapUI();
        WebHttp.Instance.RequestClanJoin((int)ClanJoinStatusType.Leave);

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedClanKickOut : Type : [ SC_CLAN_KICKOUT ] result : {0}, orderUserSeq : {1}, targetUserSeq {2}\n",
            (ClanManageResultType)result, orderUserSeq, targetUserSeq);
#endif // SHOW_LOG
    }

    private void ReceivedClanApproval() {
        int result = GetUInt8();
        int clanID = (int)GetUInt32();
        string clanName = GetString();
        if (result == (int)ClanJoinResultType.Approval) {
            WebHttp.Instance.RequestClanJoin((int)ClanJoinStatusType.Join);
            WebHttp.Instance.RequestClanChattingList(clanID);
            WebHttp.Instance.RequestClanChatBlockUserList();
            WebHttp.Instance.RequestClanBossInfo(null);
            WebHttp.Instance.RequestClanJoinUserList();

            string colorClanName = string.Format("<color=aqua>{0}</color>", clanName);
            if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
                if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ClanInfo) {
                    WebHttp.Instance.RequestClanInfo(clanID, LobbySubType.ClanInfo);

                    PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29015"), colorClanName), PopupNormalIconType.Clan, null);
                }
                else if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Clan) {
                    WebHttp.Instance.RequestClanInfo(clanID, LobbySubType.None);

                    PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29015"), colorClanName), PopupNormalIconType.Clan, null);
                    LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
                }
                else if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.ZoneMap) {
                    WebHttp.Instance.RequestClanInfo(clanID, LobbySubType.None);

                    PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29015"), colorClanName), PopupNormalIconType.Clan, null);
                }
                else {
                    WebHttp.Instance.RequestClanInfo(clanID, LobbySubType.None);

                    LobbySceneUIManager.Instance.AddNoticeCallbackList((Action okCallback) => PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29015"), colorClanName), PopupNormalIconType.Clan, okCallback));
                }
            }
            else {
                WebHttp.Instance.RequestClanInfo(clanID, LobbySubType.None);

                LobbySceneUIManager.Instance.AddNoticeCallbackList((Action okCallback) => PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29015"), colorClanName), PopupNormalIconType.Clan, okCallback));
            }
        }
        else if (result == (int)ClanJoinResultType.Refusal) {

        }
        else if (result == (int)ClanJoinResultType.Registered) {

        }
        else if (result == (int)ClanJoinResultType.ClanClassError) {

        }
        else if (result == (int)ClanJoinResultType.MemberOver) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29098")), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.LogOut) {

        }
        else if (result == (int)ClanJoinResultType.JoinRequesterOver) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29215")), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.JoinRequesterReject) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29033")), PopupNormalIconType.Clan, null);
        }
        else if (result == (int)ClanJoinResultType.AlreadyJoinRequest) {
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), string.Format(TStrings.Instance.FindString("Clan_29088")), PopupNormalIconType.Clan, null);
        }

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedClanApproval : Type : [ SC_CLAN_APPROVAL ] result : {0}\t clanName : {1}\n", result, clanName);
#endif // SHOW_LOG
    }

    private void ReceivedClanBroadcastApproval() {
        int newUserSeq = (int)GetUInt32();
        int loginStateType = GetUInt8();
        string newUserName = GetString();
        int newUserClanClass = GetUInt8();
        int newUserLevel = GetUInt16();
        int point = (int)GetUInt32();
        int pvpTier = GetUInt16();

        DateTime now = DateTime.Now;
        long nowStr = now.Millisecond;

        if (newUserSeq != PlayerManager.Instance.UserSeq) {
            MClanUserInfo clanUserInfo = new MClanUserInfo();
            clanUserInfo.m_userseq = newUserSeq;
            clanUserInfo.m_nickname = newUserName;
            clanUserInfo.m_level = newUserLevel;
            clanUserInfo.m_tier = pvpTier;
            clanUserInfo.m_point = point;
            clanUserInfo.m_class = newUserClanClass;
            clanUserInfo.m_regTime_utc = nowStr;
            clanUserInfo.m_login_status = loginStateType;

            PlayerManager.Instance.AddClanUserInfo(clanUserInfo);
            PlayerManager.Instance.AddClanMemberCount();
        }

        RefreshClanMemberTapUI();
#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedClanBroadcastApproval : Type : [ SC_BROADCAST_CLAN_APPROVAL ]\n newUserSeq : {0}\t loginStateType : {1}\t newUserName : {2}\t clanlevel : {3}\t newuserlevel : {4}\t point : {5}\t pvptier : {6}\t, reg_time : {7}\n",
            newUserSeq, loginStateType, newUserName, newUserClanClass, newUserLevel, point, pvpTier, nowStr);
#endif // SHOW_LOG
    }

    private void ReceivedClanNotify() {
        int clanID = (int)GetUInt32();
        int messageType = GetUInt16();
        int fromUserSeq = (int)GetUInt32();
        string fromName = GetString();
        int toUserSeq = (int)GetUInt32();
        int avataID = GetUInt16();
        int bgId = GetUInt16();
        int pinId = GetUInt16();
        int level = GetUInt16();
        string toName = GetString();
        string message = GetString();
        int condition = (int)GetUInt32(); // from, to 형식 이 아닌 것들 ex) 레벨 달성, 승점 조건, 가입 조건

        ClanData clanData = PlayerManager.Instance.ClanData;
        DateTime now = DateTime.UtcNow;

        MClanJoinUser clanJoinUser = new MClanJoinUser();
        if (messageType == (int)ClanMessageType.ClanJoinRequest) {
            clanJoinUser.m_userseq = toUserSeq;
            clanJoinUser.m_nickname = toName;
            clanJoinUser.m_avatar = avataID;
            clanJoinUser.m_avatarbg = bgId;
            clanJoinUser.m_avatarpin = pinId;
            clanJoinUser.m_level = level;
            clanJoinUser.m_reg_time = Common.ConvertDateTimeUTCToJavaMilliseconds(now);
            clanJoinUser.m_message = message;

            clanData._clanJoinRequestList.Add(clanJoinUser);
        }
        else if (messageType == (int)ClanMessageType.ClanJoinRequestReject) {
            clanJoinUser = clanData.FindClanJoinUser(toUserSeq);
            clanData._clanJoinRequestList.Remove(clanJoinUser);
        }
        else if (messageType == (int)ClanMessageType.ClanJoinRequestAccept) {
            clanJoinUser = clanData.FindClanJoinUser(toUserSeq);
            clanData._clanJoinRequestList.Remove(clanJoinUser);
        }

        MClanChatInfo clanChatInfo = new MClanChatInfo();
        clanChatInfo.m_type = messageType;
        clanChatInfo.m_from_userseq = fromUserSeq;
        clanChatInfo.m_from_nickname = fromName;
        clanChatInfo.m_to_userseq = toUserSeq;
        clanChatInfo.m_to_avatar = avataID;
        clanChatInfo.m_to_level = level;
        clanChatInfo.m_to_nickname = toName;
        clanChatInfo.m_message = message;
        clanChatInfo.m_condition = condition;
        clanChatInfo.m_createTime = Common.ConvertDateTimeUTCToJavaMilliseconds(now);
        clanChatInfo.m_to_avatarbg = bgId;
        clanChatInfo.m_to_avatarpin = pinId;

        clanData._clanChatInfos.Add(clanChatInfo);

        UpdateClanChattingListItem();
        UpdateClanMiniChattingListItem();
        UpdateClanJoinRequestList();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedClanNotify : Type : [ SC_CLAN_NOTIFY ]\n clanID : {0}\t messageType : {1}\t fromUserSeq : {2}\t fromName : {3}\t toUserSeq : {4}\t avataID : {5}\t level : {6}\t toName : {7}\t message : {8}\t condition : {9}\n",
            clanID, messageType, fromUserSeq, fromName, toUserSeq, avataID, level, toName, message, condition);
#endif
    }

    private void ReceivedChangeClanSettingDesc() {
        int result = GetUInt8();
        string clanDesc = GetString();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanDesc : Type : [ SC_CLAN_CHANGE_INTRO ]\n result : {0}\t changeClanDesc : {1}\n",
            (ClanManageResultType)result, clanDesc);
#endif

        PlayerManager.Instance.ClanMessage = clanDesc;
        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanSettingName() {
        int result = GetUInt8();
        string clanName = GetString();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanName : Type : [ SC_CLAN_CHANGE_CLANNAME ]\n result : {0}\t changeClanName : {1}\n",
            (ClanManageResultType)result, clanName);
#endif
        if (result == 3) { // 클랜명 중복
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29112"), PopupNormalIconType.Clan, null);
            _isClanNameOverlap = true;
        }
        else {
            PlayerManager.Instance.ClanName = clanName;
        }

        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanSettingNotice() {
        int result = GetUInt8();
        string clanNotice = GetString();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanSettingNotice : Type : [ SC_CLAN_CHANGE_PROMOTION ]\n result : {0}\t changeClanNotice : {1}\n",
            (ClanManageResultType)result, clanNotice);
#endif

        PlayerManager.Instance.ClanNotice = clanNotice;
        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanSettingJoinType() {
        int result = GetUInt8();
        int jointType = GetUInt8();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanJoinType : Type : [ SC_CLAN_CHANGE_TYPE ]\n result : {0}\t changeClanJoinType : {1}\n",
            (ClanManageResultType)result, jointType);
#endif

        PlayerManager.Instance.ClanJoinType = jointType;
        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanSettingFlag() {
        int result = GetUInt8();
        int patternIndex = GetUInt16();
        int symbolIndex = GetUInt16();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanFlag : Type : [ SC_CLAN_CHANGE_FLAG ]\n result : {0}\t changePatternIndex : {1}\t changeSymbolIndex {2}\n",
            (ClanManageResultType)result, patternIndex, symbolIndex);
#endif

        PlayerManager.Instance.ClanPatternIndex = patternIndex;
        PlayerManager.Instance.ClanSymbolIndex = symbolIndex;
        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanSettingWinscore() {
        int result = GetUInt8();
        int winscore = (int)GetUInt32();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanWinscore : Type : [ SC_CLAN_CHANGE_VICTORYPOINT ]\n result : {0}\t winScore : {1}\n",
            (ClanManageResultType)result, winscore);
#endif

        PlayerManager.Instance.ClanWinScore = winscore;
        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanSettingLanguage() {
        int result = GetUInt8();
        int languageType = GetUInt8();
        string language = TClanLanguages.Instance.Find(languageType)._name;

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanLanguage : Type : [ SC_CLAN_CHANGE_LANGUAGE ]\n result : {0}\t winScore : {1}\n",
            (ClanManageResultType)result, language);
#endif  

        PlayerManager.Instance.ClanLanguage = languageType;
        RefreshClanCommonUI();
    }

    private void ReceivedChangeClanMemberClass() {
        int result = GetUInt8();
        int orderUserSeq = (int)GetUInt32();
        int targetUserSeq = (int)GetUInt32();
        int changeClass = GetUInt8();
        int modifyType = GetUInt8();

        PlayerManager.Instance.ChangeClanMemberClass(orderUserSeq, targetUserSeq, changeClass, modifyType);

        if (GoToPrevUI != null) {
            GoToPrevUI();
            GoToPrevUI = null;
        }

        UpdateClanMemberListItem(targetUserSeq);

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanClass : Type : [ SC_CLAN_CHANGE_CLASS ]\n result : {0}\t orderUserSeq : {1}\t targetUserSeq : {2}\t classLevel : {3}\t modifyType : {4}\n",
            (ClanManageResultType)result, orderUserSeq, targetUserSeq, changeClass, modifyType);
#endif  
    }

    private void ReceivedClanDissolve() {
        int result = GetUInt8();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedDissolveClan : Type : [ SC_CLAN_DISSOLVE ]\t result : {0}\n", (ClanManageResultType)result);
#endif  
    }

    private void ReceivedChangeClanMaster() {
        int result = GetUInt8();
        int orderUserSeq = (int)GetUInt32();
        int orderClanClass = GetUInt8();
        int targetUserSeq = (int)GetUInt32();
        int targetClanClass = GetUInt8();

        PlayerManager.Instance.ChangeClanMaster(orderUserSeq, orderClanClass, targetUserSeq, targetClanClass);

        if (UpdateUICallback != null) {
            UpdateUICallback();
        }

        if (_changeClanMatserCallback != null) {
            _changeClanMatserCallback();
            _changeClanMatserCallback = null;
        }

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceiveChangeClanMaster : Type : [ SC_CLAN_CHANGE_MASTER ]\n result : {0}\t orderUserSeq : {1}\t orderClanClass : {2}\t targetUSerSeq : {3}\t targetClanClass : {4}\n",
            (ClanManageResultType)result, orderUserSeq, orderClanClass, targetUserSeq, targetClanClass);
#endif  
    }

    private void ReceivedClanLoginMemberList() {
        int clanMemberCnt = GetUInt8();

        Dictionary<int, int> clanMemberLoginDict = new Dictionary<int, int>();
        for (int i = 0, count = clanMemberCnt; i < count; ++i) {
            int userSeq = (int)GetUInt32();
            int loginState = GetUInt8();
            clanMemberLoginDict.Add(userSeq, loginState);
        }

        List<MClanUserInfo> clanUserInfos = PlayerManager.Instance.ClanData._clanUserInfos;
        List<int> userSeqList = clanMemberLoginDict.Keys.ToList();
        List<int> loginStateList = clanMemberLoginDict.Values.ToList();

        for (int i = 0, count = clanUserInfos.Count; i < count; ++i) {
            for (int k = 0, count2 = userSeqList.Count; k < count2; ++k) {
                if (clanUserInfos[i].m_userseq == userSeqList[k]) {
                    clanUserInfos[i].m_login_status = loginStateList[k];
                    break;
                }
            }
        }

        RefreshClanMemberTapUI();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceiveClanLoginMemberList : Type : [ SC_CLAN_MEMBERLIST ]\n");
#endif 
    }

    private void ReceivedClanRefusal() {
        int result = GetUInt8();
        string clanName = GetString();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceiveClanLoginMemberList : Type : [ SC_CLAN_MEMBERLIST ]\t result : {0}, clanName : {1}\n", result, clanName);
#endif 
    }

    private void ReceivedChangeClanSettingWar() {
        int result = GetUInt8();
        int warAttendType = GetUInt8();

#if SHOW_LOG
        Debug.LogFormat("TCPSocketManager : ReceivedChangeClanSettingWar : Type : [ SC_CLAN_CHANGE_ATTENDSTATUS ]\n result : {0}\t _isClanWarAttend : {1}\n",
            (ClanManageResultType)result, warAttendType);
#endif

        PlayerManager.Instance.ClanWarAttendType = warAttendType;
        RefreshClanCommonUI();
    }

    private void ReceivedButDoNothing() {
        //Do Nothing
    }

    private void ReceivedClanWarBattleProgress() {
        int attackerUID = (int)GetUInt32();
        int vsClanID = (int)GetUInt32();
        int vsUserUID = (int)GetUInt32();
        int progress = (int)GetUInt8();

        UpdateClanWarProgress(attackerUID, vsClanID, vsUserUID, progress);
    }

    private void ReceivedClanWarEnemyOnline() {
        int memberCount = (int)GetUInt16();

        List<int> attackerUID = new();
        List<int> defenderUIDs = new();
        List<int> progressList = new();

        for (int i = 0; i < memberCount; ++i) {
            int userIndex = (int)GetUInt32();
            int vsUserIndex = (int)GetUInt32();
            int progress = (int)GetUInt8();
            attackerUID.Add(userIndex);
            defenderUIDs.Add(vsUserIndex);
            progressList.Add(progress);
        }

        RefreshClanWarMap(attackerUID, defenderUIDs, progressList);
    }

    private void ReceivedClanWarHistory() {
        int vsClanID = (int)GetUInt32();
        int vsUserIndex = (int)GetUInt32();

        List<int> cardidList = new();
        List<int> cardhpList = new();

        for (int i = 0, count = Common.teamMemberMax; i < count; ++i) {
            int cardid = (int)GetUInt32();
            cardidList.Add(cardid);
            int cardhp = (int)GetUInt32();
            cardhpList.Add(cardhp);
        }
        int userIndex = (int)GetUInt32();
        int getPoint = (int)GetUInt32();

        UpdateClanWarHistory(vsClanID, vsUserIndex, cardidList, cardhpList, userIndex, getPoint);
    }

    private void ReceivedClanWarRevival() {
        int vsClanID = (int)GetUInt32();

        ClanWarRevive(vsClanID);
    }

    private void ProcReceivePackets(Type receiveType) {
        if (Type.SC_KEEPALIVE == receiveType) { ReceiveKeepAlive(); }
        else if (Type.SC_GAMELOGIN == receiveType) { ReceivedGameServerLogin(); }
        else if (Type.SC_DUPLICATELOGIN == receiveType) { ReceivedDuplicateLogin(); }
        else if (Type.SC_CLAN_JOIN == receiveType) { ReceivedClanJoin(); }
        else if (Type.SC_CLAN_NOTIFY == receiveType) { ReceivedClanNotify(); }
        else if (Type.SC_BROADCAST_CLAN_APPROVAL == receiveType) { ReceivedClanBroadcastApproval(); }
        else if (Type.SC_CLAN_APPROVAL == receiveType) { ReceivedClanApproval(); }
        else if (Type.SC_NOTIFY_LOGOUT == receiveType) { ReceivedClanMemberState(); }
        else if (Type.SC_CLAN_KICKOUT == receiveType) { ReceivedClanKickOut(); }
        else if (Type.SC_CLAN_CHANGE_INTRO == receiveType) { ReceivedChangeClanSettingDesc(); }
        else if (Type.SC_CLAN_CHANGE_CLANNAME == receiveType) { ReceivedChangeClanSettingName(); }
        else if (Type.SC_CLAN_CHANGE_TYPE == receiveType) { ReceivedChangeClanSettingJoinType(); }
        else if (Type.SC_CLAN_CHANGE_FLAG == receiveType) { ReceivedChangeClanSettingFlag(); }
        else if (Type.SC_CLAN_CHANGE_VICTORYPOINT == receiveType) { ReceivedChangeClanSettingWinscore(); }
        else if (Type.SC_CLAN_CHANGE_LANGUAGE == receiveType) { ReceivedChangeClanSettingLanguage(); }
        else if (Type.SC_CLAN_CHANGE_CLASS == receiveType) { ReceivedChangeClanMemberClass(); }
        else if (Type.SC_CLAN_DISSOLVE == receiveType) { ReceivedClanDissolve(); }
        else if (Type.SC_CLAN_CHANGE_MASTER == receiveType) { ReceivedChangeClanMaster(); }
        else if (Type.SC_CLAN_MEMBERLIST == receiveType) { ReceivedClanLoginMemberList(); }
        else if (Type.SC_CLAN_CHANGE_PROMOTION == receiveType) { ReceivedChangeClanSettingNotice(); }
        else if (Type.SC_CLAN_REFUSAL == receiveType) { ReceivedClanRefusal(); }
        else if (Type.SC_CLAN_CHANGE_ATTENDSTATUS == receiveType) { ReceivedChangeClanSettingWar(); }
        else if (Type.SC_CLAN_BATTLE_ON == receiveType) { ReceivedButDoNothing(); }
        else if (Type.SC_CLAN_BATTLE_OFF == receiveType) { ReceivedButDoNothing(); }
        else if (Type.SC_CLAN_BATTLE_PROGRESS == receiveType) { ReceivedClanWarBattleProgress(); }
        else if (Type.SC_CLAN_LIVE_MEMBERS == receiveType) { ReceivedClanWarEnemyOnline(); }
        else if (Type.SC_CLAN_BATTLE_END == receiveType) { ReceivedClanWarHistory(); }
        else if (Type.SC_CLAN_BATTLE_REVIVAL == receiveType) { ReceivedClanWarRevival(); }
    }

    #endregion // Receive
}
