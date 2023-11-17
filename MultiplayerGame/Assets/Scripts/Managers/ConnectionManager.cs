using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;
using System.Linq;

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

    [SerializeField] string hostIP = "127.0.0.1";
    [SerializeField] int port = 9050;
    [SerializeField] string myIP;

    [Header("Pinging")]
    [SerializeField] float ping;
    [SerializeField] float pingCounter;
    bool isPinging;
    [SerializeField] bool clientIsConnected;

    IPEndPoint ipep;
    EndPoint remote;

    [Header("Debug")]
    [SerializeField] bool connectAtStart = true;
    [SerializeField] bool reconnect = false;
    [SerializeField] bool disconnect = false;

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
            #region Check if client is still connected every X time

            if (pingCounter > 0)
            {
                pingCounter -= 0.1f;
            }
            else
            {
                pingCounter = ping;

                // PING
                string pingMsg = "ping";
                data = Encoding.ASCII.GetBytes(pingMsg);

                socket.SendTo(data, recv, SocketFlags.None, remote);

                isPinging = true;
            }

            #endregion

            // Probar meter esto en el UPDATE normal y guardar el ultimo ReceiveFrom en un string???

            // Receive data
            data = new byte[1024];
            recv = socket.ReceiveFrom(data, ref remote);

            Debug.Log(remote.ToString() + " : " + Encoding.ASCII.GetString(data, 0, recv)); // Log received data

            #region Check Connection Msg

            clientIsConnected = true;

            if (isPinging)
            {
                if (Encoding.ASCII.GetString(data, 0, recv) != "pong")
                {
                    clientIsConnected = false;
                }
            }

            #endregion

            // Answer data
            string answer = "SERVER: " + Encoding.ASCII.GetString(data, 0, recv);
            data = Encoding.ASCII.GetBytes(answer);

            socket.SendTo(data, recv, SocketFlags.None, remote);
        }
    }

    void ClientThreadUpdate()
    {
        try
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            remote = sender;

            // Send data
            string welcome = "Connection Stablished";
            data = Encoding.ASCII.GetBytes(welcome);
            socket.SendTo(data, data.Length, SocketFlags.None, ipep);

            while (true)
            {
                // Receive Data
                data = new byte[1024];
                recv = socket.ReceiveFrom(data, ref remote);

                if (Encoding.ASCII.GetString(data, 0, recv) == "ping")
                {
                    string pongMsg = "pong";
                    data = Encoding.ASCII.GetBytes(pongMsg);
                    socket.SendTo(data, data.Length, SocketFlags.None, ipep);
                }

                Debug.Log(Encoding.ASCII.GetString(data, 0, recv)); // Log received data
            }
        }
        catch
        {
            Debug.Log("Server is Disconnected");
            EndConnection();
        }
    }

    #endregion

    #region Start / Update / etc

    void Start()
    {
        //myIP = GetLocalIPv4();
        myIP = GetLocalIPAddress();

        pingCounter = ping;

        if (connectAtStart)
        {
            StartConnection();
        }
    }

    void Update()
    {
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
