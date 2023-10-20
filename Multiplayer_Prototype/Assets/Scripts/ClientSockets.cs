using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using TMPro;
using System;

public class ClientSockets : MonoBehaviour
{
    [SerializeField] socketType typeOfSocket;

    Socket serverSocket;

    Thread threadClient;

    byte[] data = new byte[1024];

    int recv;

    [SerializeField] bool localHostIP = false;
    [SerializeField] int port;

    IPEndPoint ipep;

    EndPoint Remote;

    [Header("UI Feedback")]
    [SerializeField] Image img;
    bool connected = false;

    [SerializeField] TMP_Text textOnScreen;
    string messagesTexts;

    [SerializeField] TMP_InputField textInput;
    [SerializeField] TMP_Text textTypeConnection;

    public string nickname = "";

    void Start()
    {
        // ======= Sockets =======

        switch (typeOfSocket)
        {
            case socketType.TCP:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                textTypeConnection.text = "TCP";
                break;

            case socketType.UDP:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                textTypeConnection.text = "UDP";
                break;
        }

        // ======= Addresses =======

        if (!localHostIP)
        {
            ipep = new IPEndPoint(IPAddress.Any, port);
        }
        else
        {
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        }

        // ======= Binding =======

        serverSocket.Bind(ipep);

        // ======= Thread =======

        threadClient = new Thread(ThreadUpdate);

        threadClient.Start();
    }

    void ThreadUpdate()
    {
        switch (typeOfSocket)
        {
            case socketType.TCP:
                try
                {
                    Debug.Log("Connecting to server ...");
                    serverSocket.Connect(ipep);

                    connected = true;
                }
                catch (SocketException e)
                {
                    Debug.Log("Failed to connect to server: " + e.ToString());

                    connected = false;
                    return;
                }

                // == Send data ==
                string wlcm = "TCP Connection Stablished (" + nickname + ")";
                data = Encoding.ASCII.GetBytes(wlcm);
                serverSocket.Send(data);

                while (connected)
                {
                    // == Receive data ==

                    recv = serverSocket.Receive(data);
                    string message = Encoding.ASCII.GetString(data, 0, recv);
                    Debug.Log("Data received: " + message);

                    messagesTexts += message + " " + DateTime.Now + "\n";
                }
                break;
            case socketType.UDP:

                IPEndPoint sender;
                if (!localHostIP)
                {
                    sender = new IPEndPoint(IPAddress.Any, 0);
                }
                else
                {
                    sender = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
                }

                Remote = (EndPoint)(sender);

                connected = true;

                // == Send data ==
                string welcome = "UDP Connection Stablished (" + nickname + ")";
                data = Encoding.ASCII.GetBytes(welcome);
                serverSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

                while (connected)
                {
                    // Receive Data
                    data = new byte[1024];
                    recv = serverSocket.ReceiveFrom(data, ref Remote);

                    Debug.Log("Data received from " + Remote.ToString() + " : " + Encoding.ASCII.GetString(data, 0, recv));

                    messagesTexts += Remote.ToString() + " " + Encoding.ASCII.GetString(data, 0, recv) + " " + DateTime.Now + "\n";
                }
                break;
        }
    }

    private void Update()
    {
        if (img != null)
        {
            if (connected) { img.color = Color.green; }
            else { img.color = Color.red; }
        }

        if (textOnScreen != null)
        {
            textOnScreen.text = messagesTexts;
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrWhiteSpace(textInput.text))
        {
            // == Send data ==
            Debug.Log("Sending: " + textInput.text);
            string msg;

            switch (typeOfSocket)
            {
                case socketType.TCP:

                    msg = nickname + " : " + textInput.text;
                    data = Encoding.ASCII.GetBytes(msg);
                    serverSocket.Send(data);

                    break;
                case socketType.UDP:

                    msg = "as " + nickname + " said : " + textInput.text;
                    data = Encoding.ASCII.GetBytes(msg);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

                    break;
            }
            textInput.text = "";
        }
    }

    enum socketType
    {
        UDP,
        TCP
    }

    public void ResetConnection()
    {
        DisconnectSocket();

        textInput.text = "";

        Start();
    }

    void DisconnectSocket()
    {
        threadClient.Abort();

        if (serverSocket.Connected)
        {
            serverSocket.Shutdown(SocketShutdown.Both);

            connected = false;
        }

        serverSocket.Close();
    }

    //private void OnApplicationQuit()
    //{
    //    DisconnectSocket();
    //    Debug.Log("Cerrando Socket");
    //}

    public void ChangeSocketConnection()
    {
        if(typeOfSocket == socketType.TCP)
        {
            typeOfSocket = socketType.UDP;
            textTypeConnection.text = "UDP";
        }
        else
        {
            typeOfSocket = socketType.TCP;
            textTypeConnection.text = "TCP";
        }
    }
}
