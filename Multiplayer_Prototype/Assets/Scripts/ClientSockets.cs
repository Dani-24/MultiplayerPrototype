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
    [SerializeField] string pendingMessage = "";

    void Start()
    {
        // ======= Sockets =======

        switch (typeOfSocket)
        {
            case socketType.TCP:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                break;

            case socketType.UDP:
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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

                // == Send data ==      (Meter en el while)
                string wlcm = "TCP Client Bombardeando Renfe";
                data = Encoding.ASCII.GetBytes(wlcm);
                serverSocket.Send(data);

                while (connected)
                {
                    lock (pendingMessage)
                    {
                        if (pendingMessage != "")
                        {
                            // == Send data ==
                            data = Encoding.ASCII.GetBytes(pendingMessage);
                            serverSocket.Send(data);

                            Debug.Log("Sending: " + pendingMessage);

                            pendingMessage = "";
                        }
                    }

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

                // == Send data ==      (Meter en el while)
                string welcome = "UDP Client Bombardeando Renfe";
                data = Encoding.ASCII.GetBytes(welcome);
                serverSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

                while (connected)
                {
                    if (pendingMessage != "")
                    {
                        // == Send data ==
                        data = Encoding.ASCII.GetBytes(pendingMessage);
                        serverSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

                        Debug.Log("Sending: " + pendingMessage);

                        pendingMessage = "";
                    }

                    data = new byte[1024];
                    recv = serverSocket.ReceiveFrom(data, ref Remote);

                    Debug.Log("Data received from " + Remote.ToString() + " : " + Encoding.ASCII.GetString(data, 0, recv));

                    messagesTexts += Remote.ToString() + " : " + Encoding.ASCII.GetString(data, 0, recv) + " " + DateTime.Now + "\n";
                }
                break;
        }
        //DisconnectSocket();
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

        if(Input.GetKeyDown(KeyCode.Return)) {
            lock (pendingMessage)
            {
                pendingMessage = textInput.text;
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

        pendingMessage = textInput.text = "";

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
}
