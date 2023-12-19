using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    #region Propierties

    [Header("Instance Name")]
    public string userName;
    string defaultUserName = "Player";

    [Tooltip("Is Server or Client")]
    public bool isHosting = false;

    [Header("Connection Propierties")]

    [SerializeField] bool isConnected = false;

    Socket socket;

    Thread serverReceiveThread, clientReceiveThread, serverSendThread, clientSendThread;

    byte[] data = new byte[1024];

    [SerializeField] string hostIP = "127.0.0.1"; string defaultIP = "127.0.0.1";
    [SerializeField] int port = 9050; int defaultPort = 9050;
    [SerializeField] string myIP;

    [Header("Packages")]
    [SerializeField] int packageDataSize = 1024;
    [SerializeField] float delayBetweenPckgs = 0.1f;
    float delay;
    [SerializeField] bool enablePckLogs = true;

    [Header("Pinging")]
    [SerializeField] float pingCounter;
    float pingInterval;
    bool isPinging;

    [SerializeField] bool serverIsConnected;
    [SerializeField] bool clientIsConnected;

    IPEndPoint ipep;
    EndPoint remote;

    [SerializeField] AudioClip startClip;
    [SerializeField] AudioClip endClip;
    AudioSource audioSource;
    bool playJoin, playEnd;

    [Header("Debug")]
    [SerializeField] bool connectAtStart = true;
    public bool reconnect = false;
    public bool disconnect = false;

    #region NET Data

    [Header("DEBUG NET DATA (Don't Edit)")]
    [SerializeField] bool connectionStablished = false;
    string sceneName;

    [SerializeField] int ownPlayerNetID = -1;
    [SerializeField] PlayerPackage ownPlayerPck;

    [SerializeField] List<PlayerPackage> playerPackages = new List<PlayerPackage>();

    Color _alphaTcolor;
    Color _betaTcolor;
    Color _NEWalphaTcolor;
    Color _NEWbetaTcolor;
    bool changeColor;

    bool pendingToClean = false;

    #endregion

    #endregion

    bool cleanPaint = false;
    [SerializeField] GameObject sceneRoot;

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

        if (enablePckLogs) Debug.Log("Received Pck: " + pck.type);

        return pck;
    }

    Package WritePackage(Pck_type type)
    {
        Package pck = new Package();
        pck.netID = ownPlayerNetID;
        pck.IP = myIP;
        pck.type = type;
        pck.currentScene = sceneName;

        if (enablePckLogs) Debug.Log("Sending Pck " + pck.type);

        switch (type)
        {
            case Pck_type.Player:       // CLIENT

                if (ownPlayerPck != null)
                {
                    pck.playerPck = ownPlayerPck;
                    pck.playerPck.userName = userName;
                }

                break;
            case Pck_type.PlayerList:   // SERVER

                // Update own player
                for (int i = 0; i < playerPackages.Count; i++)
                {
                    if (playerPackages[i].netID == ownPlayerPck.netID)
                    {
                        playerPackages[i] = ownPlayerPck;
                        break;
                    }
                }

                pck.playersListPck = playerPackages;

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

        if (pck.currentScene != sceneName)
        {
            // CHANGE SCENE
        }

        switch (pck.type)
        {
            case Pck_type.Player:       // SERVER

                bool alreadyExists = false;

                for (int i = 0; i < playerPackages.Count; i++)
                {
                    if (pck.playerPck.netID == playerPackages[i].netID)
                    {
                        playerPackages[i] = pck.playerPck;
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    playerPackages.Add(pck.playerPck);
                }

                break;
            case Pck_type.PlayerList:   // CLIENT

                playerPackages = pck.playersListPck;

                break;
            case Pck_type.Ping: // Funcion aqui que interprete PingPackage

                break;
            case Pck_type.Connection:   // Mensajes de conexión

                pckLog += " || ";
                if (pck.connPck.isAnswer) { pckLog += "Answering to: "; };
                pckLog += pck.connPck.message;

                playJoin = true;

                if (pck.connPck.setColor)
                {
                    changeColor = true;
                    _NEWalphaTcolor = pck.connPck.alphaColor;
                    _NEWbetaTcolor = pck.connPck.betaColor;
                }

                break;
        }

        if (enablePckLogs) Debug.Log(pckLog);
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

        try
        {
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
        catch
        {
            Debug.Log("Incorrect IP/Port format");
        }
    }

    private void EndConnection(string debugLog = "Connection Ended")
    {
        if (isConnected)
        {
            Debug.Log(debugLog);

            playEnd = true;

            isConnected = false;
            disconnect = false;
            connectionStablished = false;
            pendingToClean = true;
            serverIsConnected = false;
            clientIsConnected = false;

            cleanPaint = true;

            playerPackages.Clear();

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

                if (connectionStablished /*&& delay > delayBetweenPckgs*/)
                {
                    try
                    {
                        MemoryStream sendPStream = new MemoryStream();
                        Package pPck = WritePackage(Pck_type.PlayerList);
                        pPck.user = Network_User.Server;
                        sendPStream = SerializeJson(pPck);

                        socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, remote);
                        delay = 0;

                    }
                    catch
                    {
                        Debug.Log("Client has disconnected (Send)");

                        if (clientIsConnected)
                        {
                            EndConnection();
                        }
                    }
                }

                #endregion
            }
        }
    }

    void ServerReceiveThreadUpdate()
    {
        Debug.Log("Waiting for a Client...");
        try
        {
            while (true)
            {
                // Receive data
                data = new byte[packageDataSize];
                socket.ReceiveFrom(data, ref remote);

                // Manage Package
                MemoryStream receiveStream = new MemoryStream(data);
                Package receivedPck = DeserializeJson(receiveStream);
                ReadPackage(receivedPck);

                if (receivedPck.type == Pck_type.Connection)
                {
                    MemoryStream sendStream = new MemoryStream();

                    Package answerPck = WritePackage(Pck_type.Connection);
                    answerPck.connPck.message = receivedPck.connPck.message;
                    answerPck.user = Network_User.Server;
                    answerPck.connPck.isAnswer = true;

                    // Set Team Colors
                    answerPck.connPck.setColor = true;
                    answerPck.connPck.alphaColor = _alphaTcolor;
                    answerPck.connPck.betaColor = _betaTcolor;

                    sendStream = SerializeJson(answerPck);
                    socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, remote);

                    connectionStablished = true;
                }

                if (!clientIsConnected)
                {
                    clientIsConnected = true;
                    cleanPaint = true;
                }
            }
        }
        catch (SystemException e)
        {
            Debug.Log("Client has disconnected (Receive)" + e.ToString());

            if (clientIsConnected)
            {
                EndConnection();
            }
        }
    }

    void ClientSendThreadUpdate()
    {
        try
        {
            #region Send player start position

            MemoryStream sendStream = new MemoryStream();
            Package pck = WritePackage(Pck_type.Connection);
            pck.connPck.message = "Connection Stablished";
            pck.user = Network_User.Client;
            sendStream = SerializeJson(pck);

            socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, ipep);

            connectionStablished = true;

            #endregion

            while (true)
            {
                #region Update/Send Player Input

                if (connectionStablished /*&& delay > delayBetweenPckgs*/ && serverIsConnected)
                {
                    MemoryStream sendPStream = new MemoryStream();
                    Package pPck = WritePackage(Pck_type.Player);
                    pPck.user = Network_User.Client;
                    sendPStream = SerializeJson(pPck);

                    socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, ipep);
                    delay = 0;
                }

                #endregion
            }
        }
        catch (SystemException e)
        {
            Debug.Log(e.ToString());
            EndConnection("Server is Disconnected (Send)");
        }
    }

    void ClientReceiveThreadUpdate()
    {
        while (true)
        {
            try
            {
                // Receive Data
                data = new byte[packageDataSize];
                socket.ReceiveFrom(data, ref remote);

                MemoryStream receiveStream = new MemoryStream(data);

                // Manage Package
                ReadPackage(DeserializeJson(receiveStream));

                if (!serverIsConnected)
                {
                    serverIsConnected = true;
                    cleanPaint = true;
                }
            }
            catch (SystemException e)
            {
                // REVISAR ESTA PARTE YA Q SI EL CLIENT SE CONECTA PRIMERO LE SALE Q ESTÁ CONECTADO A ALGO CUANDO NO LO ESTÁ

                Debug.Log(e.ToString());

                if (serverIsConnected)
                {
                    EndConnection();
                }
            }
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

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        #region Connection toggles

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

        if (pendingToClean)
        {
            CleanPlayers();
        }

        if (cleanPaint)
        {
            sceneRoot.BroadcastMessage("CleanPaint");
            cleanPaint = false;
        }

        #endregion

        #region Get values from UI

        string getIP = UI_Manager.Instance.userIP;
        int getPort = UI_Manager.Instance.userPort;
        string usName = UI_Manager.Instance.userName;

        if (getIP != "") { hostIP = getIP; } else { hostIP = defaultIP; }
        if (getPort != 0) { port = getPort; } else { port = defaultPort; }
        if (usName != "") { userName = usName; } else { userName = defaultUserName; }

        #endregion

        // Delay between sending Packages
        delay += Time.deltaTime;
        // Ping
        pingCounter -= Time.deltaTime;

        // Update Own Player Info
        if (SceneManagerScript.Instance.GetOwnPlayerInstance() != null && SceneManagerScript.Instance.gameState == SceneManagerScript.GameState.Gameplay)
        {
            ownPlayerNetID = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().networkID;
            ownPlayerPck = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().GetPlayerPck();
            ownPlayerPck.userName = userName;
        }

        // Update Player List
        if (isConnected)
        {
            if (isHosting && playerPackages.Count == 0)
            {
                playerPackages.Add(ownPlayerPck);
            }

            for (int i = 0; i < playerPackages.Count; i++)
            {
                bool alreadyExists = false;
                for (int j = 0; j < SceneManagerScript.Instance.playersOnScene.Count; j++)
                {
                    if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().networkID == playerPackages[i].netID)
                    {
                        SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().SetPlayerInfoFromPck(playerPackages[i]);
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    GameObject newP = SceneManagerScript.Instance.CreateNewPlayer(false, playerPackages[i].position);
                    newP.GetComponent<PlayerNetworking>().networkID = playerPackages[i].netID;
                }
            }
        }

        sceneName = SceneManager.GetActiveScene().name;

        #region Get Server Colors

        _alphaTcolor = SceneManagerScript.Instance.GetTeamColor("Alpha");
        _betaTcolor = SceneManagerScript.Instance.GetTeamColor("Beta");

        if (changeColor)
        {
            SceneManagerScript.Instance.SetColors(_NEWalphaTcolor, _NEWbetaTcolor);
            changeColor = false;
        }

        #endregion

        #region AUDIO

        if (playJoin)
        {
            playJoin = false;
            audioSource.clip = startClip;
            audioSource.Play();
        }
        if (playEnd)
        {
            playEnd = false;
            audioSource.clip = endClip;
            audioSource.Play();
        }

        #endregion
    }

    void CleanPlayers()
    {
        SceneManagerScript.Instance.DeleteAllNotOwnedPlayers();
        pendingToClean = false;
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
    PlayerList,
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
    public string currentScene;

    public PlayerPackage playerPck = null;                                  // Esto lo devuelve el client
    public List<PlayerPackage> playersListPck = new List<PlayerPackage>();  // Esto lo devuelve el Server

    public PingPackage pingPck = null;
    public ConnectionPackage connPck = null;
}

[System.Serializable]
public class PlayerPackage
{
    public string userName;
    public string teamTag;
    public int netID;

    public Vector3 position;
    public Quaternion rotation;
    public Vector2 moveInput;   // Revisar si hace falta este actualmente
    public Vector3 camRot;      // Usar para camara de muerte??? No, q la camara de muerte sea una orbital random y ya, Quitar esto entonces.

    public bool running = false;
    public bool jumping = false;

    public bool shooting = false;
    public bool shootingSub = false;

    public UnityEngine.Random.State wpRNG;
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

    // Team Colors
    public bool setColor = false;
    public Color alphaColor;
    public Color betaColor;

    // Manual Disconnect
    public bool setDisconnected = false;
}

// Scenes Packages
[System.Serializable]
public class LobbyObjectsToSync
{
    // Literalmente se mueven en un único eje
    public float ratRot;
    public float clockRot;
    public float elevatorPos;

    public int jukeBoxSong;
}

#endregion