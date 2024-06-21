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

    [SerializeField] List<NetGO> newNetGOs;

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

    [Header("Data Packages")]
    public Package ownPkg = new();
    public List<Package> clientPkgs = new();

    [SerializeField] bool connectionStablished = false;
    string activeSceneName;

    [HideInInspector] public string ownTeamTagOnSceneChange = "";

    Color _alphaTcolor;
    Color _betaTcolor;
    Color _NEWalphaTcolor;
    Color _NEWbetaTcolor;
    bool changeColor;

    bool pendingToCleanPlayers = false;

    int cont = 0;

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

    public Package WritePackage()
    {
        return ownPkg;
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

        if (!isHosting && isConnected) newNetGOs = pck.sceneNetGO;

        if (isHosting)
        {
            bool alreadyExists = false;

            for (int i = 0; i < ownPkg.clientListPck.Count; i++)
            {
                if (pck.netID == ownPkg.clientListPck[i].netID)
                {
                    ownPkg.clientListPck[i] = pck;
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists) ownPkg.clientListPck.Add(pck);
        }
        else ownPkg.clientListPck = pck.clientListPck;

        if (!pck.connPck.canConnect && pck.IP == myIP)
        {
            // Limit players
            EndConnection();
            return;
        }

        if (pck.connPck != null)
        {
            changeColor = true;
            _NEWalphaTcolor = pck.connPck.alphaColor;
            _NEWbetaTcolor = pck.connPck.betaColor;
        }

        if (pck.dmGPackage.receiverID == ownPkg.netID) dmgReceivedPCKG.Add(pck.dmGPackage);

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

                if (connectionStablished)
                {
                    try
                    {
                        MemoryStream sendPStream = new MemoryStream();
                        Package pPck = WritePackage();
                        pPck.user = Network_User.Server;

                        sendPStream = SerializeJson(pPck);

                        socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, remote);
                    }
                    catch
                    {
                        Debug.Log("Client has disconnected (Send)");

                        showCommError = true;

                        if (clientIsConnected) EndConnection();
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

            if (clientIsConnected) EndConnection();
        }
    }

    void ClientSendThreadUpdate()
    {
        try
        {
            while (true)
            {
                #region Update/Send Player Input

                if (connectionStablished && delay > connectionTickRate && serverIsConnected)
                {
                    MemoryStream sendPStream = new MemoryStream();
                    Package pPck = WritePackage();
                    sendPStream = SerializeJson(pPck);
                    socket.SendTo(sendPStream.ToArray(), (int)sendPStream.Length, SocketFlags.None, ipep);
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

        userName = UI_Manager.Instance.userName;
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

        if (isHosting) ownPkg.clientListPck = clientPkgs;

        #region AUDIO

        if (cont < clientPkgs.Count - 1)
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

        if (onlinePlay && !logged) StartCoroutine(LogIn());
    }

    void UpdateGameObjects()
    {
        // Update Own Player Info
        if (SceneManagerScript.Instance.GetOwnPlayerInstance() != null)
        {
            ownPkg.netID = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().networkID;
            ownPkg.playerPck = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerNetworking>().GetPlayerPck();
            ownPkg.playerPck.userName = userName; 
            ownPkg.IP = myIP;
            ownPkg.currentScene = activeSceneName;
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

            #region Manage Player Packages

            for (int i = 0; i < ownPkg.clientListPck.Count; i++)
            {
                bool alreadyExists = false;
                for (int j = 0; j < SceneManagerScript.Instance.playersOnScene.Count; j++)
                {
                    if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().networkID == ownPkg.clientListPck[i].netID)
                    {
                        if (!SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().isOwnByThisInstance)
                        {
                            SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().SetPlayerInfoFromPck(ownPkg.clientListPck[i].playerPck);
                            alreadyExists = true;

                            // Check Client Disconnection
                            if (ownPkg.clientListPck[i].playerPck.setDisconnected || (DateTime.UtcNow - ownPkg.clientListPck[i].pckCreationTime).Seconds > disconnectionTime)
                            {
                                SceneManagerScript.Instance.DeletePlayer(SceneManagerScript.Instance.playersOnScene[j]);

                                ownPkg.clientListPck[i].playerPck.setDisconnected = true;
                                ownPkg.clientListPck[i].playerPck.userName = "DC - " + ownPkg.clientListPck[i].playerPck.userName;

                                playEnd = true;
                            }

                            break;
                        }
                        else if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerNetworking>().isOwnByThisInstance)
                        {
                            if (SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerStats>().teamTag != ownPkg.clientListPck[i].playerPck.teamTag && !isHosting)
                                SceneManagerScript.Instance.playersOnScene[j].GetComponent<PlayerStats>().ChangeTag(ownPkg.clientListPck[i].playerPck.teamTag);

                            alreadyExists = true;
                            break;
                        }
                    }
                }

                if (!alreadyExists && !ownPkg.clientListPck[i].playerPck.setDisconnected)
                {
                    GameObject newP = SceneManagerScript.Instance.CreateNewPlayer(false, new Vector3(0, 2, 0));
                    newP.GetComponent<PlayerNetworking>().networkID = ownPkg.clientListPck[i].netID;

                    SceneManagerScript.Instance.cleanPaint = true;
                }
            }

            #endregion

            #region Net GameObjects

            if (isHosting && SceneManagerScript.Instance.netGOs.Count > 0)
            {
                for (int i = 0; i < SceneManagerScript.Instance.netGOs.Count; i++)
                {
                    NetGO netGO = new()
                    {
                        id = SceneManagerScript.Instance.netGOs[i].GOid,
                        variable = SceneManagerScript.Instance.netGOs[i].netValue
                    };

                    ownPkg.sceneNetGO.Add(netGO);
                }
            }

            #endregion

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

        Package pPck = WritePackage();
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
            ReadPackage(PkgFromJson(www.downloadHandler.text));
            Debug.Log("Receiving Host Data: " + www.downloadHandler.text);
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
        Package pPck = WritePackage();
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

public enum Network_User
{
    None,
    Server,
    Client
}

[System.Serializable]
public class Package
{
    public string IP;
    public DateTime pckCreationTime = DateTime.UtcNow;
    public Network_User user;
    public int netID;
    public DateTime pckDate = DateTime.UtcNow;

    public string currentScene;

    // Package Data
    public List<NetGO> sceneNetGO = new();
    public PlayerPackage playerPck = null;
    public List<Package> clientListPck = null;
    public ConnectionPackage connPck = null;
    public List<DMGPackage> dmGPackage = null;
}

[System.Serializable]
public class PlayerPackage
{
    // Data
    public string userName;
    public string teamTag;

    public LifeState lifeState;

    // Transform
    public Vector3 position;
    public Quaternion rotation;

    public Vector3 camRot;

    // Input Actions
    public bool running = false;
    public bool jumping = false;

    public bool shooting = false;
    public bool shootingSub = false;

    public bool inputEnabled = false;

    // Weapons
    public int mainWeapon;
    public int subWeapon;

    public UnityEngine.Random.State wpRNG;

    // Force Disconnect
    public bool setDisconnected = false;
}

[System.Serializable]
public class ConnectionPackage
{
    // Game Info
    public string version;

    // Team Colors
    public Color alphaColor;
    public Color betaColor;

    // Ask server
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
public class NetGO
{
    public int id;
    public float variable;
}

#endregion