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

    [Tooltip("Is Server or Client")]
    public bool isHosting = false;

    [Header("Connection Propierties")]
    [SerializeField] bool isConnected = false;

    Socket socket;

    Thread serverReceiveThread, clientReceiveThread, serverSendThread, clientSendThread;

    byte[] data = new byte[1024];

    public bool localHost = false;

    [SerializeField] string hostIP = "127.0.0.1"; string defaultIP = "127.0.0.1";
    [SerializeField] int port = 9050; int defaultPort = 9050;
    [SerializeField] string myIP;

    [Header("Packages")]
    [SerializeField] int packageDataSize = 10240;
    [SerializeField] float delayBetweenPckgs = 0.1f;
    float delay;
    [SerializeField] bool enablePckLogs = true;

    [SerializeField][Tooltip("In sec.")] float disconnectionTime = 5;

    [SerializeField] bool serverIsConnected;
    [SerializeField] bool clientIsConnected;

    DateTime lastPckgDateTime;

    IPEndPoint ipep;
    EndPoint remote;

    [SerializeField] AudioClip startClip;
    [SerializeField] AudioClip endClip;
    AudioSource audioSource;
    bool playJoin, playEnd;

    [Header("Current Scene Online GameObjects")]
    [SerializeField] List<NetGameObject> netGOs;
    [SerializeField] List<netGO> newNetGOs;

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

    bool cleanPaint = false;
    [SerializeField] GameObject sceneRoot;

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

        #region Net Scene GameObject
        if (isHosting && netGOs.Count > 0)
        {
            for (int i = 0; i < netGOs.Count; i++)
            {
                netGO netGO = new netGO();
                netGO.id = netGOs[i].GOid;
                netGO.variable = netGOs[i].netValue;

                pck.sceneNetGO.Add(netGO);
            }
        }
        #endregion

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
                if (delay > delayBetweenPckgs)  // Eliminar este if mas adelante haciendo un buen delay enviando paquetes desde el server
                {
                    delay = 0;

                    for (int i = 0; i < playerPackages.Count; i++)
                    {
                        if (playerPackages[i].netID == ownPlayerPck.netID)
                        {
                            playerPackages[i] = ownPlayerPck;
                            break;
                        }
                    }
                }

                pck.playersListPck = playerPackages;

                break;
            case Pck_type.Connection:

                pck.connPck = new ConnectionPackage();

                break;
        }

        return pck;
    }

    void ReadPackage(Package pck)
    {
        lastPckgDateTime = pck.pckCreationTime;
        int pckDelay = (DateTime.UtcNow - lastPckgDateTime).Milliseconds;
        string pckLog = "Package from: " + pck.user + " (" + pck.IP + ") ms: " + pckDelay;

        if (pck.currentScene != sceneName)
        {
            // CHANGE SCENE
        }

        if (!isHosting && isConnected)
        {
            newNetGOs = pck.sceneNetGO;
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
            case Pck_type.Connection:   // Mensajes de conexi�n

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
                port = FindAvailablePort();

                ipep = new IPEndPoint(IPAddress.Any, port); // As server allow connections from anywhere

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

            foreach (NetGameObject n in netGOs)
            {
                n.connectedToServer = false;
            }

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

    public string GetHostIP()
    {
        return hostIP;
    }

    public int GetCurrentPort()
    {
        return port;
    }

    int FindAvailablePort()
    {
        if (localHost) return defaultPort;

        for (int i = 1; i <= 9050; i++)
        {
            if (IsPortAvailable(i))
            {
                Debug.Log("Port " + i + " is available");
                return i;
            }
        }

        throw new Exception("There is no Port Available");
    }

    bool IsPortAvailable(int _port)
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipep = new IPEndPoint(IPAddress.Any, _port); // As server allow connections from anywhere

            socket.Bind(ipep);

            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public void SetIP(string _ip)
    {
        if (_ip != "") { hostIP = _ip; } else { hostIP = defaultIP; }
    }

    public void SetPort(int _port)
    {
        if (_port != 0) { port = _port; } else { port = defaultPort; }
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
                        //delay = 0;

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

                if (connectionStablished && delay > delayBetweenPckgs && serverIsConnected)
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
        Thread.Sleep(100);

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
                Debug.Log(e.ToString());

                EndConnection();
            }
        }
    }

    #endregion

    #region Start / Update / etc

    void Start()
    {
        //myIP = GetLocalIPv4();
        myIP = GetLocalIPAddress();

        if (connectAtStart)
        {
            StartConnection();
        }

        audioSource = GetComponent<AudioSource>();

        int num = 0;
        foreach (NetGameObject n in netGOs)
        {
            n.GOid = num;
            num++;
        }
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

        userName = UI_Manager.Instance.userName;

        // Delay between sending Packages
        delay += Time.deltaTime;

        UpdateGameObjects();

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

    void UpdateGameObjects()
    {
        // Update Own Player Info
        if (SceneManagerScript.Instance.GetOwnPlayerInstance() != null/* && SceneManagerScript.Instance.gameState == SceneManagerScript.GameState.Gameplay*/)
        {
            ownPlayerNetID = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().networkID;
            ownPlayerPck = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().GetPlayerPck();
            ownPlayerPck.userName = userName;
        }

        if (isConnected)
        {
            // Net GO Update (Client only)
            if (!isHosting)
            {
                for (int i = 0; i < netGOs.Count; i++)
                {
                    for (int j = 0; j < newNetGOs.Count; j++)
                    {
                        if (netGOs[i].GOid == newNetGOs[j].id)
                        {
                            netGOs[i].connectedToServer = true;
                            netGOs[i].netValue = newNetGOs[j].variable;
                            break;
                        }
                    }
                }
            }

            if (isHosting && playerPackages.Count == 0)
            {
                playerPackages.Add(ownPlayerPck);
            }

            // Manage Player Packages
            for (int i = 0; i < playerPackages.Count; i++)
            {
                bool alreadyExists = false;
                for (int j = 0; j < SceneManagerScript.Instance.playersOnScene.Count; j++)
                {
                    if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().networkID == playerPackages[i].netID)
                    {
                        if (!SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().isOwnByThisInstance)
                        {
                            SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().SetPlayerInfoFromPck(playerPackages[i]);
                            alreadyExists = true;

                            // Check Client Disconnection
                            if (playerPackages[i].setDisconnected || (DateTime.UtcNow - playerPackages[i].playerPckCreationTime).Seconds > disconnectionTime)
                            {
                                SceneManagerScript.Instance.DeletePlayer(SceneManagerScript.Instance.playersOnScene[j]);
                                playerPackages[i].setDisconnected = true;
                                playEnd = true;
                            }

                            break;
                        }
                        else if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().isOwnByThisInstance)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                }

                if (!alreadyExists && !playerPackages[i].setDisconnected)
                {
                    GameObject newP = SceneManagerScript.Instance.CreateNewPlayer(false, playerPackages[i].position);
                    newP.GetComponent<PlayerNetworking>().networkID = playerPackages[i].netID;
                    cleanPaint = true;
                }
            }

            // Disconnect yourself (Client)
            if (!isHosting && serverIsConnected && (DateTime.UtcNow - lastPckgDateTime).Seconds > disconnectionTime)
            {
                disconnect = true;
            }
        }

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
    public List<netGO> sceneNetGO = new List<netGO>();

    public PlayerPackage playerPck = null;                                  // Esto lo devuelve el client
    public List<PlayerPackage> playersListPck = new List<PlayerPackage>();  // Esto lo devuelve el Server

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
    //public Vector2 moveInput;
    public Vector3 camRot;      // Revisar si se quiere esto aun

    public bool running = false;
    public bool jumping = false;

    public bool shooting = false;
    public bool shootingSub = false;

    public UnityEngine.Random.State wpRNG;
    public DateTime playerPckCreationTime = DateTime.UtcNow;

    public bool inputEnabled = false;
    // Manual Disconnect
    public bool setDisconnected = false;
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
}

[System.Serializable]
public class netGO
{
    public int id;
    public float variable;
}

#endregion