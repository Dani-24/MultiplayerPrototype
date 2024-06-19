using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static PlayerStats;
using System.Collections;
using UnityEngine.Networking;

public class ConnectionManager : MonoBehaviour
{
    #region Propierties

    [Header("Instance Version")] public string version;

    [Header("Instance Name")] public string userName;

    [Tooltip("Is Server or Client")] public bool isHosting = false;

    [Header("Room Specs")]
    public int maxPlayers = 8;
    public ConnectionGameplayState connGameplayState;
    public string lobbyScene;

    [Header("Connection Propierties")]
    [SerializeField] bool isConnected = false;

    Socket socket;
    Thread serverReceiveThread,
        clientReceiveThread,
        serverSendThread,
        clientSendThread;

    IPEndPoint ipep;
    EndPoint remote;

    byte[] data = new byte[1024];

    public bool localHost = false;

    [SerializeField] string hostIP = "127.0.0.1";
    string defaultIP = "127.0.0.1";
    [SerializeField] int port = 9050;
    int defaultPort = 13420;
    [SerializeField] string myIP;

    [SerializeField] bool serverIsConnected;
    [SerializeField] bool clientIsConnected;

    [Header("Packages")]
    [SerializeField] int packageDataSize = 10240;
    [SerializeField][Tooltip("Delay between Packages")] float connectionTickRate = 0.1f;
    float delay;
    DateTime lastPckgDateTime;

    [SerializeField][Tooltip("In sec.")] float disconnectionTime = 5;

    [SerializeField] List<netGO> newNetGOs;

    [Header("Online (PHP)")]
    [SerializeField] bool onlinePlay = false;
    [SerializeField] bool logged = false;
    [SerializeField] string PHP_Url = "https://citmalumnes.upc.es/~danieltr1/OnlinePlay.php";
    [SerializeField] int PHP_roomId = -1;
    [SerializeField] int PHP_userId = -1;

    public List<string> availableRooms = new();

    public bool searchRooms = false;
    public bool createRoom = false;

    [Header("LOGS")]
    [SerializeField] bool enablePckLogs = true;
    [SerializeField] bool showCommError = false;

    [Header("Debug")]
    [SerializeField] bool connectAtStart = true;
    public bool reconnect = false;
    public bool disconnect = false;

    bool changeScene;
    string sceneToChange;

    #region NET Data

    [Header("NET Data Display")]
    [SerializeField] bool connectionStablished = false;
    string activeSceneName;

    public int ownPlayerNetID = -1;
    public PlayerPackage ownPlayerPck;
    [HideInInspector] public string ownTeamTagOnSceneChange = "";

    public List<PlayerPackage> playerPackages = new List<PlayerPackage>();

    Color _alphaTcolor;
    Color _betaTcolor;
    Color _NEWalphaTcolor;
    Color _NEWbetaTcolor;
    bool changeColor;

    bool pendingToCleanPlayers = false;

    int cont = 0;

    Package randomPackageToSend = null;
    List<DMGPackage> dmgReceivedPCKG = new();

    #endregion

    [Header("Audio SFX")]
    [SerializeField] AudioClip startClip;
    [SerializeField] AudioClip endClip;
    AudioSource audioSource;
    bool playJoin, playEnd;

    #endregion

    #region Packages & Serialization

    MemoryStream SerializeJson(Package pck)
    {
        string json = JsonUtility.ToJson(pck);
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        Debug.Log(stream.Length + " bytes");

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

    string PkgToJson(Package pck)
    {
        return JsonUtility.ToJson(pck);
    }

    Package PkgFromJson(string json)
    {
        return JsonUtility.FromJson<Package>(json);
    }

    public Package WritePackage(Pck_type type)
    {
        Package pck = new Package();
        pck.netID = ownPlayerNetID;
        pck.IP = myIP;
        pck.type = type;
        pck.currentScene = activeSceneName;

        #region Net Scene GameObject
        if (isHosting && SceneManagerScript.Instance.netGOs.Count > 0)
        {
            for (int i = 0; i < SceneManagerScript.Instance.netGOs.Count; i++)
            {
                netGO netGO = new netGO();
                netGO.id = SceneManagerScript.Instance.netGOs[i].GOid;
                netGO.variable = SceneManagerScript.Instance.netGOs[i].netValue;

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
                if (delay > connectionTickRate)
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
            case Pck_type.DMG:

                pck.dmGPackage = new DMGPackage();

                break;
        }

        return pck;
    }

    void ReadPackage(Package pck)
    {
        lastPckgDateTime = pck.pckCreationTime;
        int pckDelay = (DateTime.UtcNow - lastPckgDateTime).Milliseconds;
        string pckLog = "Package from: " + pck.user + " (" + pck.IP + ") ms: " + pckDelay;

        if (!isHosting && pck.currentScene != activeSceneName)
        {
            // CHANGE SCENE
            sceneToChange = pck.currentScene;
            changeScene = true;
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

                Debug.Log(pck.playerPck.netID);

                if (!alreadyExists) playerPackages.Add(pck.playerPck);

                break;
            case Pck_type.PlayerList:   // CLIENT

                playerPackages = pck.playersListPck;

                break;
            case Pck_type.Connection:   // Mensajes de conexión

                if (!pck.connPck.canConnect && pck.IP == myIP)
                {
                    // Limit players
                    EndConnection();
                    break;
                }

                pckLog += " || ";
                if (pck.connPck.isAnswer) { pckLog += "Answering to: "; };
                pckLog += pck.connPck.message;

                if (pck.connPck.setColor)
                {
                    changeColor = true;
                    _NEWalphaTcolor = pck.connPck.alphaColor;
                    _NEWbetaTcolor = pck.connPck.betaColor;
                }

                break;
            case Pck_type.DMG:

                if (pck.dmGPackage.receiverID == ownPlayerNetID)
                    dmgReceivedPCKG.Add(pck.dmGPackage);

                break;
        }

        if (enablePckLogs) Debug.Log(pckLog);
    }

    public void SendPackage(Package pck)
    {
        randomPackageToSend = pck;
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
            DontDestroyOnLoad(this.gameObject);
        }
    }

    #endregion

    #region Connection

    public void StartConnection()
    {
        if (isConnected)
            EndConnection();

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
                port = FindAvailablePort(); //

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
            Debug.Log("Incorrect IP/Port format | Can't start a Connection");
            UI_Manager.Instance.PopUp_LogMessage("Can't start a Connection", 3);
        }
    }

    private void EndConnection(string debugLog = "Connection Ended")
    {
        if (isConnected)
        {
            Debug.Log(debugLog);

            if (!onlinePlay)
            {
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

                if (socket.Connected) socket.Shutdown(SocketShutdown.Both);

                socket.Close();
            }
            else
            {
                StartCoroutine(DisconnectPHP());
                PHP_roomId = -1;
            }

            foreach (NetGameObject n in SceneManagerScript.Instance.netGOs)
                n.connectedToServer = false;

            playerPackages.Clear();

            onlinePlay = false;
            isHosting = false;
            isConnected = false;
            disconnect = false;
            connectionStablished = false;
            pendingToCleanPlayers = true;
            serverIsConnected = false;
            clientIsConnected = false;

            SceneManagerScript.Instance.cleanPaint = true;

            if (activeSceneName != lobbyScene)
            {
                Debug.Log("Connection Lost: Returning to Lobby");

                activeSceneName = lobbyScene;
                SceneManagerScript.Instance.ChangeScene(lobbyScene);
            }
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();

        throw new Exception("No network adapters with an IPv4 address in the system!");
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
        return defaultPort; // Define always the same port

        if (localHost) return defaultPort;

        for (int i = 1; i <= 9050; i++)
        {
            int j = UnityEngine.Random.Range(0, 9050);
            if (IsPortAvailable(j))
            {
                Debug.Log("Port " + j + " is available");
                return j;
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
                        //delay = 0;
                        MemoryStream sendPStream = new MemoryStream();
                        Package pPck = WritePackage(Pck_type.PlayerList);
                        pPck.user = Network_User.Server;

                        sendPStream = SerializeJson(pPck);

                        socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, remote);

                        if (randomPackageToSend != null)
                        {
                            MemoryStream sendPStreamB = new MemoryStream();
                            sendPStreamB = SerializeJson(randomPackageToSend);

                            socket.SendTo(sendPStreamB.ToArray(), (int)sendPStreamB.Length, SocketFlags.None, remote);

                            randomPackageToSend = null;
                        }
                    }
                    catch
                    {
                        Debug.Log("Client has disconnected (Send)");

                        showCommError = true;

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
                    answerPck.IP = receivedPck.IP; // Return same IP
                    answerPck.connPck.message = receivedPck.connPck.message;
                    answerPck.user = Network_User.Server;
                    answerPck.connPck.isAnswer = true;

                    if (playerPackages.Count >= maxPlayers || connGameplayState != ConnectionGameplayState.Lobby || receivedPck.connPck.version != version)
                    {
                        answerPck.connPck.canConnect = false;
                        answerPck.connPck.message += "\n Cannot connect to the room. Check if the room is full / playing a match or running on a different version";
                    }
                    else
                    {
                        // Set Team Colors
                        answerPck.connPck.setColor = true;
                        answerPck.connPck.alphaColor = _alphaTcolor;
                        answerPck.connPck.betaColor = _betaTcolor;
                    }

                    sendStream = SerializeJson(answerPck);
                    socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, remote);

                    connectionStablished = true;
                }

                if (!clientIsConnected)
                {
                    clientIsConnected = true;
                    SceneManagerScript.Instance.cleanPaint = true;
                }
            }
        }
        catch (SystemException e)
        {
            Debug.Log("Client has disconnected (Receive)" + e.ToString());
            showCommError = true;
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
            pck.connPck.version = version;
            pck.user = Network_User.Client;
            sendStream = SerializeJson(pck);

            socket.SendTo(sendStream.ToArray(), (int)sendStream.Length, SocketFlags.None, ipep);

            connectionStablished = true;

            #endregion

            while (true)
            {
                #region Update/Send Player Input

                if (connectionStablished && delay > connectionTickRate && serverIsConnected)
                {
                    MemoryStream sendPStream = new MemoryStream();
                    Package pPck = WritePackage(Pck_type.Player);
                    pPck.user = Network_User.Client;
                    sendPStream = SerializeJson(pPck);

                    socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, ipep);
                    delay = 0;

                    if (randomPackageToSend != null)
                    {
                        MemoryStream sendPStreamB = new MemoryStream();
                        sendPStreamB = SerializeJson(randomPackageToSend);

                        socket.SendTo(sendPStreamB.ToArray(), (int)sendPStreamB.Length, SocketFlags.None, remote);

                        randomPackageToSend = null;
                    }
                }

                #endregion
            }
        }
        catch (SystemException e)
        {
            Debug.Log(e.ToString());
            showCommError = true;
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
                    SceneManagerScript.Instance.cleanPaint = true;
                }
            }
            catch (SystemException e)
            {
                Debug.Log(e.ToString());
                showCommError = true;
                EndConnection("Server is Disconnected (Receive)");
            }
        }
    }

    #endregion

    #region Start / Update / etc

    void Start()
    {
        version = Application.version;

        myIP = GetLocalIPAddress();

        if (connectAtStart)
            StartConnection();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        #region Connection toggles

        if (reconnect)
        {
            if (!isConnected) StartConnection();
            reconnect = false;
        }

        if (disconnect) EndConnection();
        if (pendingToCleanPlayers) CleanPlayers();

        if (showCommError)
        {
            showCommError = false;
            UI_Manager.Instance.PopUp_LogMessage("A connection error has ocurred", 3.5f, true, lobbyScene);
        }

        #endregion

        userName = UI_Manager.Instance.userName;    // this should only update when the username is updated instead of every frame

        activeSceneName = SceneManager.GetActiveScene().name;

        delay += Time.deltaTime;

        UpdateGameObjects();

        if (changeScene)
        {
            changeScene = false;
            SceneManagerScript.Instance.netGOs.Clear();
            SceneManagerScript.Instance.ChangeScene(sceneToChange);
            sceneToChange = "";
        }

        if (activeSceneName != lobbyScene) connGameplayState = ConnectionGameplayState.Playing; else connGameplayState = ConnectionGameplayState.Lobby;

        // DMG
        for (int i = 0; i < dmgReceivedPCKG.Count; i++)
        {
            SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().OnDMGReceive(dmgReceivedPCKG[i].cause, dmgReceivedPCKG[i].dmg, dmgReceivedPCKG[i].dealer);
            dmgReceivedPCKG.Remove(dmgReceivedPCKG[i]);
        }

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

        if (cont < playerPackages.Count)
        {
            playJoin = true;
            cont++;
        }

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

        // PHP
        if (searchRooms)
        {
            searchRooms = false;
            StartCoroutine(SearchRoom());
        }

        if (createRoom)
        {
            createRoom = false;
            if (PHP_roomId == -1) StartCoroutine(HostRoom());
        }

        if (onlinePlay && delay > connectionTickRate && PHP_roomId != -1 && PHP_userId != -1)
        {
            delay = 0;

            if (isHosting)
            {
                StartCoroutine(SendHostData());
                StartCoroutine(ReceiveClientData());
            }
            else
            {
                StartCoroutine(SendClientData());
                StartCoroutine(ReceiveHostData());
            }
        }

        if (onlinePlay && !logged)
        {
            StartCoroutine(LogIn());
        }
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
                for (int i = 0; i < SceneManagerScript.Instance.netGOs.Count; i++)
                {
                    for (int j = 0; j < newNetGOs.Count; j++)
                    {
                        if (SceneManagerScript.Instance.netGOs[i].GOid == newNetGOs[j].id)
                        {
                            SceneManagerScript.Instance.netGOs[i].connectedToServer = true;
                            SceneManagerScript.Instance.netGOs[i].netValue = newNetGOs[j].variable;
                            break;
                        }
                    }
                }
            }

            if (isHosting && playerPackages.Count == 0) playerPackages.Add(ownPlayerPck);

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
                                playerPackages[i].userName = "DC - " + playerPackages[i].userName;

                                playEnd = true;
                            }

                            break;
                        }
                        else if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().isOwnByThisInstance)
                        {
                            if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerStats>().teamTag != playerPackages[i].teamTag && !isHosting)
                            {
                                SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerStats>().ChangeTag(playerPackages[i].teamTag);
                            }
                            alreadyExists = true;
                            break;
                        }
                    }
                }

                if (!alreadyExists && !playerPackages[i].setDisconnected)
                {
                    GameObject newP = SceneManagerScript.Instance.CreateNewPlayer(false, new Vector3(0, 2, 0));
                    newP.GetComponent<PlayerNetworking>().networkID = playerPackages[i].netID;

                    SceneManagerScript.Instance.cleanPaint = true;
                }
            }

            // Disconnect yourself (Client)
            if (!isHosting && serverIsConnected && (DateTime.UtcNow - lastPckgDateTime).Seconds > disconnectionTime) disconnect = true;
        }

    }

    void CleanPlayers()
    {
        SceneManagerScript.Instance.DeleteAllNotOwnedPlayers();
        pendingToCleanPlayers = false;
    }

    private void OnApplicationQuit()
    {
        if (isConnected) EndConnection();
    }

    #endregion

    #region PHP - SQL

    public void JoinRoom(int id)
    {
        PHP_roomId = id;
        isHosting = false;
        onlinePlay = isConnected = true;
    }

    public IEnumerator LogIn()
    {
        logged = true;
        WWWForm form = new();

        form.AddField("methodToCall", "Log In");

        form.AddField("userName", userName);

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        PHP_userId = int.Parse(www.downloadHandler.text);

        if (PHP_userId == -1) logged = false;
    }

    // ROOM

    public IEnumerator HostRoom()
    {
        Debug.Log("Hosting a Room on Online Mode");

        isHosting = isConnected = onlinePlay = true;

        WWWForm form = new();

        form.AddField("methodToCall", "Host Room");

        form.AddField("host", userName);
        form.AddField("timeStamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        try
        {
            PHP_roomId = int.Parse(www.downloadHandler.text);
            Debug.Log("Room Id asigned: " + PHP_roomId);
        }
        catch
        {
            Debug.Log(www.downloadHandler.text);
        }
    }

    public IEnumerator DisconnectPHP()
    {
        WWWForm form = new();

        if (isHosting)
        {
            form.AddField("methodToCall", "Close Room");
            form.AddField("userId", PHP_userId);
            form.AddField("roomId", PHP_roomId);

            UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

            yield return www.SendWebRequest();

            Debug.Log("Room Closed " + www.downloadHandler.text);
        }
        else // Client
        {
            form.AddField("methodToCall", "Disconnect Client");
            form.AddField("userId", PHP_userId);
            form.AddField("roomId", PHP_roomId);

            UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

            yield return www.SendWebRequest();

            Debug.Log("Client disconnected from Online Play " + www.downloadHandler.text);
        }
    }

    public IEnumerator SearchRoom()
    {
        WWWForm form = new();
        form.AddField("methodToCall", "Search Room");

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonData = www.downloadHandler.text;
            string[] rows = jsonData.Split('\n');

            availableRooms.Clear();

            foreach (string row in rows)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    try
                    {
                        if (int.Parse(row.Substring(0, 2)) == -1)
                            Debug.Log(row.Substring(2));
                        break;
                    }
                    catch
                    {
                        // No error "-1". Code can continue
                    }

                    availableRooms.Add(row);
                    Debug.Log(row);
                }
            }
            UI_Manager.Instance.CastFunctionOnChildren("RefreshRoomList");
        }
        else Debug.Log("Error: " + www.error);
    }

    // HOST

    public IEnumerator SendHostData()
    {
        WWWForm form = new();

        form.AddField("methodToCall", "Send Host Data");

        form.AddField("roomId", PHP_roomId);

        form.AddField("timeStamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        Package pPck = WritePackage(Pck_type.PlayerList);
        pPck.user = Network_User.Server;

        form.AddField("data", PkgToJson(pPck));

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
    }

    public IEnumerator ReceiveHostData()
    {
        WWWForm form = new();

        form.AddField("methodToCall", "Receive Host Data");

        form.AddField("roomId", PHP_roomId);

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        try
        {
            HostData rowData = JsonUtility.FromJson<HostData>(www.downloadHandler.text);
            ReadPackage(PkgFromJson(rowData.Data));

            Debug.Log("Receiving Host Data: " + rowData.Data);
        }
        catch
        {
            Debug.Log("Cannot Receiving Host Data: " + www.downloadHandler.error);
        }
    }

    // CLIENT

    public IEnumerator SendClientData()
    {
        WWWForm form = new();

        form.AddField("methodToCall", "Send Client Data");
        form.AddField("roomId", PHP_roomId);
        form.AddField("timeStamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        form.AddField("clientId", PHP_userId);

        MemoryStream sendPStream = new MemoryStream();
        Package pPck = WritePackage(Pck_type.Player);
        pPck.user = Network_User.Client;

        form.AddField("data", PkgToJson(pPck));

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
    }

    public IEnumerator ReceiveClientData()
    {
        WWWForm form = new();

        form.AddField("methodToCall", "Receive Client Data");

        form.AddField("roomId", PHP_roomId);

        UnityWebRequest www = UnityWebRequest.Post(PHP_Url, form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonData = www.downloadHandler.text;
            string[] rows = jsonData.Split('\n');

            availableRooms.Clear();

            foreach (string row in rows)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    try
                    {
                        ClientData rowData = JsonUtility.FromJson<ClientData>(row);
                        ReadPackage(PkgFromJson(rowData.Data));
                        Debug.Log("Receiving Client Data: " + rowData.Data);
                    }
                    catch
                    {
                        Debug.Log("Receiving Client Data: " + www.downloadHandler.text);
                    }
                }
            }
        }
        else Debug.Log("PHP Error: " + www.error);
    }

    public class HostData
    {
        public string Id;
        public string Room_Id;
        public string Data;
        public string Date;
    }

    public class ClientData
    {
        public string Id;
        public string Room_Id;
        public string Client_Id;
        public string Data;
        public string Date;
    }

    #endregion
}

public enum ConnectionGameplayState
{
    Lobby,
    Loading,
    Playing
}

#region Package Classes

public enum Pck_type
{
    Player,
    PlayerList,
    Connection,
    DMG
}

public enum Network_User
{
    None,
    Server,
    Client
}

[System.Serializable]
public class Package
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

    public DMGPackage dmGPackage = null;
}

[System.Serializable]
public class PlayerPackage
{
    public string userName;
    public string teamTag;
    public int netID;

    public LifeState lifeState;

    public Vector3 position;
    public Quaternion rotation;

    public Vector3 camRot;

    public bool running = false;
    public bool jumping = false;

    public bool shooting = false;
    public bool shootingSub = false;

    public int mainWeapon;
    public int subWeapon;

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
    public string version;
    public bool isAnswer = false;

    // Team Colors
    public bool setColor = false;
    public Color alphaColor;
    public Color betaColor;

    // Ask server if the client can connect to the server
    public bool canConnect = true;
}

[System.Serializable]
public class DMGPackage
{
    public float dmg;
    public string cause;
    public string dealer;
    public int receiverID;
}

[System.Serializable]
public class netGO
{
    public int id;
    public float variable;
}

#endregion