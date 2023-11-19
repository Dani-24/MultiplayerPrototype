using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;
using System.Linq;
using System.IO;
using System;

public class ConnectionManager : MonoBehaviour
{
    #region Propierties

    [Tooltip("Is Server or Client")]
    public bool isHosting = false;

    [Header("Connection Propierties")]

    [SerializeField] bool isConnected = false;

    Socket socket;

    Thread serverReceiveThread, clientReceiveThread, serverSendThread, clientSendThread;

    byte[] data = new byte[1024];
    int recv;

    [SerializeField] string hostIP = "127.0.0.1"; string defaultIP = "127.0.0.1";
    [SerializeField] int port = 9050; int defaultPort = 9050;
    [SerializeField] string myIP;

    [Header("Packages")]
    [SerializeField] int packageDataSize = 1024;
    [SerializeField] float delayBetweenPckgs = 0.1f;
    float delay;

    [Header("Pinging")]
    [SerializeField] float pingCounter;
    float pingInterval;
    bool isPinging;
    [SerializeField] bool clientIsConnected;

    IPEndPoint ipep;
    EndPoint remote;

    [Header("Debug")]
    [SerializeField] bool connectAtStart = true;
    public bool reconnect = false;
    public bool disconnect = false;

    #region NET Data

    [Header("DEBUG NET DATA")]

    PlayerToAdd playerToAdd = new PlayerToAdd();
    bool addPlayer = false;

    Vector3 ownPlayerPos;
    int ownPlayerNetID = -1;
    PlayerPackage ownPlayerPck;

    int notOwnPlayerNetID;
    PlayerPackage notOwnPlayerPck;

    bool ownPlayerSend = false;
    bool notOwnPlayerReceived = false;

    Package receivedPck;

    Color _alphaTcolor, _betaTcolor, _NEWalphaTcolor, _NEWbetaTcolor;
    bool changeColor;


    #endregion

    #endregion

    #region Packages & Serialization

    MemoryStream SerializeJson(Package pck)
    {
        string json = JsonUtility.ToJson(pck);
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        return stream;
    }

    Package DeserializeJson(MemoryStream stream)
    {
        var pck = new Package();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        pck = JsonUtility.FromJson<Package>(json);

        Debug.Log("Received Pck: " + pck.type);

        return pck;
    }

    Package WritePackage(Pck_type type)
    {
        Package pck = new Package();
        pck.IP = myIP;
        pck.type = type;

        Debug.Log("Sending Pck " + pck.type /*+ " at " + pck.pckCreationTime*/);

        switch (type)
        {
            case Pck_type.Player:

                if (ownPlayerPck != null)
                {
                    pck.playerPck = ownPlayerPck;
                }

                break;
            case Pck_type.Ping:

                pck.pingPck = new PingPackage();

                // Fill pingPck ??

                break;
            case Pck_type.Connection:

                pck.connPck = new ConnectionPackage();

                break;
        }

        return pck;
    }

    void ReadPackage(Package pck)
    {
        int pckDelay = (DateTime.UtcNow - pck.pckCreationTime).Milliseconds;
        string pckLog = "Package from: " + pck.user + " (" + pck.IP + ") ms: " + pckDelay;

        switch (pck.type)
        {
            case Pck_type.Player:   // Funcion de Player Networking que reciba un PlayerPackage

                notOwnPlayerNetID = pck.netID;
                notOwnPlayerPck = pck.playerPck;

                break;
            case Pck_type.Ping: // Funcion aqui que interprete PingPackage

                break;
            case Pck_type.Connection:   // Mensajes de conexión
                pckLog += " || ";
                if (pck.connPck.isAnswer) { pckLog += "Answering to: "; };
                pckLog += pck.connPck.message;

                if (pck.connPck.createPlayer)
                {
                    notOwnPlayerReceived = true;
                    Debug.Log("Creating a Player from Network");

                    playerToAdd.id = pck.netID;
                    playerToAdd.own = false;
                    playerToAdd.position = pck.connPck.playerPos;

                    addPlayer = true;
                }

                if (pck.connPck.setColor)
                {
                    changeColor = true;
                    _NEWalphaTcolor = pck.connPck.alphaColor;
                    _NEWbetaTcolor = pck.connPck.betaColor;
                }

                break;
        }

        Debug.Log(pckLog);
    }

    #endregion

    #region Manager Instance

    private static ConnectionManager _instance;
    public static ConnectionManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    #region Connection

    public void StartConnection()
    {
        if (isConnected)
        {
            EndConnection();
        }

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        if (isHosting)
        {
            ipep = new IPEndPoint(IPAddress.Any, port);             // As server allow connections from anywhere

            socket.Bind(ipep);

            serverReceiveThread = new Thread(ServerReceiveThreadUpdate);
            serverReceiveThread.Start();

            serverSendThread = new Thread(ServerSendThreadUpdate);
            serverSendThread.Start();
        }
        else
        {
            ipep = new IPEndPoint(IPAddress.Parse(hostIP), port);       // As client set the server IP

            clientReceiveThread = new Thread(ClientReceiveThreadUpdate);
            clientReceiveThread.Start();

            clientSendThread = new Thread(ClientSendThreadUpdate);
            clientSendThread.Start();
        }

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        remote = (EndPoint)(sender);

        isConnected = true;
    }

    private void EndConnection(string debugLog = "Connection Ended")
    {
        Debug.Log(debugLog);

        if (isConnected)
        {
            isConnected = false;
            disconnect = false;
            ownPlayerSend = false;
            notOwnPlayerReceived = false;

            if (isHosting)
            {
                serverSendThread.Abort();
                serverReceiveThread.Abort();
            }
            else
            {
                clientSendThread.Abort();
                clientReceiveThread.Abort();
            }

            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }

            socket.Close();
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    // Both get local IP actually seems to do the same . . .
    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
    }
    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    #endregion

    #region Threads Loops

    void ServerSendThreadUpdate()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        remote = (EndPoint)(sender);

        while (true)
        {
            if (clientIsConnected)
            {
                #region Send Own Player Data

                if (ownPlayerSend && delay > delayBetweenPckgs)
                {
                    MemoryStream sendPStream = new MemoryStream();
                    Package pPck = WritePackage(Pck_type.Player);
                    pPck.netID = ownPlayerNetID;
                    pPck.user = Network_User.Client;
                    sendPStream = SerializeJson(pPck);

                    socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, remote);
                    delay = 0;
                }

                #endregion
            }
        }
    }

    void ServerReceiveThreadUpdate()
    {
        Debug.Log("Waiting for a Client...");

        while (true)
        {
            // Receive data
            data = new byte[packageDataSize];
            recv = socket.ReceiveFrom(data, ref remote);

            // Manage Package
            MemoryStream receiveStream = new MemoryStream(data);
            receivedPck = DeserializeJson(receiveStream);
            ReadPackage(receivedPck);

            if (receivedPck.type == Pck_type.Connection)
            {
                MemoryStream sendStream = new MemoryStream();

                Package answerPck = WritePackage(Pck_type.Connection);
                answerPck.connPck.message = receivedPck.connPck.message;
                answerPck.user = Network_User.Server;
                answerPck.connPck.isAnswer = true;

                if (receivedPck.connPck.createPlayer) // Devolver el player del server al client nuevo
                {
                    answerPck.connPck.createPlayer = true;
                    answerPck.connPck.playerPos = ownPlayerPos;
                    answerPck.netID = ownPlayerNetID;
                    ownPlayerSend = true;

                    answerPck.connPck.setColor = true;
                    answerPck.connPck.alphaColor = _alphaTcolor;
                    answerPck.connPck.betaColor = _betaTcolor;
                }

                sendStream = SerializeJson(answerPck);
                socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, remote);
            }

            clientIsConnected = true;
        }
    }

    void ClientSendThreadUpdate()
    {
        try
        {
            #region Stablish connection + send player start position

            MemoryStream sendStream = new MemoryStream();
            Package pck = WritePackage(Pck_type.Connection);
            pck.connPck.message = "Connection Stablished";
            pck.connPck.createPlayer = true;
            pck.connPck.playerPos = ownPlayerPos;
            pck.netID = ownPlayerNetID;
            pck.user = Network_User.Client;
            sendStream = SerializeJson(pck);

            socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, ipep);

            ownPlayerSend = true;

            #endregion

            while (true)
            {
                // Ping Counters
                //if (pingCounter <= 0)
                //{
                //    pingCounter = pingInterval;

                //    // SEND PING PACKAGE
                //}


                #region Update/Send Player Input

                if (ownPlayerSend && delay > delayBetweenPckgs)
                {
                    MemoryStream sendPStream = new MemoryStream();
                    Package pPck = WritePackage(Pck_type.Player);
                    pPck.netID = ownPlayerNetID;
                    pPck.user = Network_User.Client;
                    sendPStream = SerializeJson(pPck);

                    socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, ipep);
                    delay = 0;
                }

                #endregion

            }
        }
        catch
        {
            EndConnection("Server is Disconnected (Send)");
        }
    }

    void ClientReceiveThreadUpdate()
    {
        try
        {
            while (true)
            {
                // Receive Data
                data = new byte[packageDataSize];
                recv = socket.ReceiveFrom(data, ref remote);

                // Manage Package
                MemoryStream receiveStream = new MemoryStream(data);
                ReadPackage(DeserializeJson(receiveStream));
            }
        }
        catch
        {
            EndConnection("Server is Disconnected (Receive)");
        }
    }

    #endregion

    #region Start / Update / etc

    void Start()
    {
        //myIP = GetLocalIPv4();
        myIP = GetLocalIPAddress();

        pingInterval = pingCounter;

        if (connectAtStart)
        {
            StartConnection();
        }
    }

    void Update()
    {
        // Connection toggles
        if (reconnect)
        {
            if (!isConnected)
            {
                StartConnection();
            }
            reconnect = false;
        }

        if (disconnect)
        {
            EndConnection();

            // A la tercera saldrá bien :D (Deberia arreglar esto... o no)
            SceneManagerScript.Instance.DeleteAllNotOwnedPlayers();
            SceneManagerScript.Instance.DeleteAllNotOwnedPlayers();
            SceneManagerScript.Instance.DeleteAllNotOwnedPlayers();
        }

        // Get values from UI
        string getIP = GetComponent<UI_Manager>().GetIpFromInput();
        int getPort = GetComponent<UI_Manager>().GetPortFromInput();

        if (getIP != "") { hostIP = GetComponent<UI_Manager>().GetIpFromInput(); } else { hostIP = defaultIP; }
        if (getPort != 0) { port = GetComponent<UI_Manager>().GetPortFromInput(); } else { port = defaultPort; }

        delay += Time.deltaTime;

        // Ping
        pingCounter -= Time.deltaTime;

        // Add player from the other instance
        if (addPlayer)
        {
            GameObject newP = SceneManagerScript.Instance.CreateNewPlayer(playerToAdd.own, playerToAdd.position);
            newP.GetComponent<PlayerNetworking>().networkID = playerToAdd.id;

            addPlayer = false;
        }

        // Update Own Player Info
        if (SceneManagerScript.Instance.GetOwnPlayerInstance() != null)
        {
            ownPlayerPos = SceneManagerScript.Instance.GetOwnPlayerInstance().transform.position;
            ownPlayerNetID = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().networkID;

            ownPlayerPck = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().GetPlayerPck();
        }

        // Update Not Own Player Info
        if (notOwnPlayerReceived)
        {
            for (int i = 0; i < SceneManagerScript.Instance.playersOnScene.Count; i++)
            {
                if (SceneManagerScript.Instance.playersOnScene[i].GetComponent<PlayerNetworking>().networkID == notOwnPlayerNetID)
                {
                    SceneManagerScript.Instance.playersOnScene[i].GetComponent<PlayerNetworking>().SetPlayerInfoFromPck(notOwnPlayerPck);
                    break;
                }
            }
        }

        // Get Server Colors
        _alphaTcolor = SceneManagerScript.Instance.GetTeamColor("Alpha");
        _betaTcolor = SceneManagerScript.Instance.GetTeamColor("Beta");

        if (changeColor)
        {
            SceneManagerScript.Instance.SetColors(_NEWalphaTcolor, _NEWbetaTcolor);
            changeColor = false;
        }
    }

    private void OnApplicationQuit()
    {
        if (isConnected)
        {
            EndConnection();
        }
    }

    #endregion
}

#region Package Classes

public enum Pck_type
{
    Player,
    Ping,
    Connection
}

public enum Network_User
{
    None,
    Server,
    Client
}

[System.Serializable]
class Package
{
    public Pck_type type;
    public string IP;
    public DateTime pckCreationTime = DateTime.UtcNow;
    public Network_User user;
    public int netID;

    public PlayerPackage playerPck = null;
    public PingPackage pingPck = null;
    public ConnectionPackage connPck = null;
}

[System.Serializable]
public class PlayerPackage
{
    public string teamTag;

    public Vector3 position;
    public Vector2 moveInput;
    public Vector2 camRot;

    public bool running = false;
    public bool jumping = false;

    public bool shooting = false;
    public bool shootingSub = false;
}

[System.Serializable]
public class PingPackage
{
    public string msgP;
}

[System.Serializable]
public class ConnectionPackage
{
    public string message;
    public bool isAnswer = false;

    // At first connection pass Player Transform
    public bool createPlayer = false;
    public Vector3 playerPos;

    // Team Colors
    public bool setColor = false;
    public Color alphaColor;
    public Color betaColor;
}

[System.Serializable]
public class PlayerToAdd
{
    public int id;
    public bool own;
    public Vector3 position;
}

#endregion