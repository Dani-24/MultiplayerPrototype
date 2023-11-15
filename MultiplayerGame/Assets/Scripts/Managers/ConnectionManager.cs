using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;

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

    [SerializeField] string IP = "127.0.0.1";
    [SerializeField] int port = 9050;

    IPEndPoint ipep;
    EndPoint remote;

    [Header("Debug")]
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
            ipep = new IPEndPoint(IPAddress.Parse(IP), port);       // As client set the server IP

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
            data = new byte[1024];
            recv = socket.ReceiveFrom(data, ref remote);

            Debug.Log(remote.ToString() + " : " + Encoding.ASCII.GetString(data, 0, recv)); // Log received data

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
            string welcome = "Client Connection Stablished";
            data = Encoding.ASCII.GetBytes(welcome);
            socket.SendTo(data, data.Length, SocketFlags.None, ipep);

            while (true)
            {
                // Receive Data
                data = new byte[1024];
                recv = socket.ReceiveFrom(data, ref remote);

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

    void Start()
    {
        StartConnection();
    }

    void Update()
    {
        if (reconnect)
        {
            if(!isConnected)
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
}
