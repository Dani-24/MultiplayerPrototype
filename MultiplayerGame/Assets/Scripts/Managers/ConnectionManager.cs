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

    Thread serverThread, clientThread;

    byte[] data = new byte[1024];
    int recv;

    [SerializeField] string hostIP = "127.0.0.1"; string defaultIP = "127.0.0.1";
    [SerializeField] int port = 9050; int defaultPort = 9050;
    [SerializeField] string myIP;

    [SerializeField] int packageDataSize = 1024;

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

        pck.pckCreationTime = DateTime.UtcNow;
        pck.IP = myIP;

        Debug.Log("Sending Pck " + pck.type + " at " + pck.pckCreationTime);

        switch (type)
        {
            case Pck_type.Player:

                pck.playerPck = new PlayerPackage();

                // Fill playerPck ??

                pck.type = Pck_type.Player;

                break;
            case Pck_type.Ping:

                pck.pingPck = new PingPackage();

                // Fill pingPck ??

                pck.type = Pck_type.Ping;

                break;
            case Pck_type.Connection:

                pck.connPck = new ConnectionPackage();
                pck.type = Pck_type.Connection;

                break;
        }

        return pck;
    }

    void ReadPackage(Package pck)
    {
        string pckLog = "Package from: " + pck.user + " (" + pck.IP + ") created at: " + pck.pckCreationTime;

        switch (pck.type)
        {
            case Pck_type.Player:

                // Funcion de Player Networking que reciba un PlayerPackage

                break;
            case Pck_type.Ping:

                // Funcion aqui que interprete PingPackage

                break;
            case Pck_type.Connection:
                if (pck.connPck.isAnswer) { pckLog += " || Answering to:"; };
                pckLog += " " + pck.connPck.message;
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

            serverThread = new Thread(ServerThreadUpdate);
            serverThread.Start();
        }
        else
        {
            ipep = new IPEndPoint(IPAddress.Parse(hostIP), port);       // As client set the server IP

            clientThread = new Thread(ClientThreadUpdate);
            clientThread.Start();
        }

        isConnected = true;
    }

    private void EndConnection(string debugLog = "Connection Ended")
    {
        Debug.Log(debugLog);

        if (isConnected)
        {
            isConnected = false;
            disconnect = false;

            if (isHosting)
            {
                serverThread.Abort();
            }
            else
            {
                clientThread.Abort();
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

    void ServerThreadUpdate()
    {
        Debug.Log("Waiting for a Client...");

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        remote = (EndPoint)(sender);

        while (true)
        {
            // Receive data
            data = new byte[packageDataSize];
            recv = socket.ReceiveFrom(data, ref remote);

            // Manage Package
            MemoryStream receiveStream = new MemoryStream(data);
            Package rPack = DeserializeJson(receiveStream);
            ReadPackage(rPack);

            // Answer data
            MemoryStream sendStream = new MemoryStream();
            Package answerPck = WritePackage(Pck_type.Connection);
            answerPck.connPck.message = rPack.connPck.message;
            answerPck.user = Network_User.Server;
            answerPck.connPck.isAnswer = true;
            sendStream = SerializeJson(answerPck);

            socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, remote);
        }
    }

    void ClientThreadUpdate()
    {
        try
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            remote = sender;

            MemoryStream sendStream = new MemoryStream();
            Package pck = WritePackage(Pck_type.Connection);
            pck.connPck.message = "Connection Stablished";
            pck.user = Network_User.Client;
            sendStream = SerializeJson(pck);

            socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, ipep);

            while (true)
            {
                // Ping Counters
                //if (pingCounter <= 0)
                //{
                //    pingCounter = pingInterval;

                //    // SEND PING PACKAGE
                //}

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
            EndConnection("Server is Disconnected");
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
        }

        // Get values from UI
        string getIP = GetComponent<UI_Manager>().GetIpFromInput();
        int getPort = GetComponent<UI_Manager>().GetPortFromInput();

        if (getIP != "") { hostIP = GetComponent<UI_Manager>().GetIpFromInput(); } else { hostIP = defaultIP; }
        if (getPort != 0) { port = GetComponent<UI_Manager>().GetPortFromInput(); } else { port = defaultPort; }

        // Ping
        pingCounter -= Time.deltaTime;
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
    Server,
    Client
}

[System.Serializable]
class Package
{
    public Pck_type type;
    public string IP;
    public DateTime pckCreationTime;
    public Network_User user;

    public PlayerPackage playerPck = null;
    public PingPackage pingPck = null;
    public ConnectionPackage connPck = null;
}

[System.Serializable]
public class PlayerPackage
{
    public Vector2 moveInput;

    public bool isRunning = false;
    public bool isJumping = false;

    public bool isShooting = false;
    public bool isSubWeapon = false;
}

[System.Serializable]
public class PingPackage
{
    public string debug = "Bombardeen Renfe";
}

[System.Serializable]
public class ConnectionPackage
{
    public string message;
    public bool isAnswer = false;
}

#endregion