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

    Socket newSocket;

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

    int i = 1;

    void Start()
    {
        // ======= Sockets =======

        switch (typeOfSocket)
        {
            case socketType.TCP:
                newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                break;

            case socketType.UDP:
                newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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

        newSocket.Bind(ipep);

        // ======= Thread =======

        threadClient = new Thread(ThreadUpdate);

        threadClient.Start();
    }

    void ThreadUpdate()
    {
        Debug.Log(i); i++;

        switch (typeOfSocket)
        {
            case socketType.TCP:

                if (!connected)
                {
                    try
                    {
                        Debug.Log("Connecting to server . . .");
                        newSocket.Connect(ipep);

                        connected = true;
                    }
                    catch (SocketException e)
                    {
                        Debug.Log("Failed to connect to server: " + e.ToString());

                        connected = false;
                        return;
                    }
                }

                // ESTO TIENE QUE ESTAR EN BUCLE
                recv = newSocket.Receive(data);
                string message = Encoding.ASCII.GetString(data, 0, recv);
                Debug.Log("Data received: " + message);

                messagesTexts += message + " " + DateTime.Now + "\n";

                break;
            case socketType.UDP:

                if (!connected)
                {
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

                    string welcome = "UDP Client Bombardeando Renfe";
                    data = Encoding.ASCII.GetBytes(welcome);
                    newSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

                }

                data = new byte[1024];
                recv = newSocket.ReceiveFrom(data, ref Remote);

                Debug.Log("Message received from "+ Remote.ToString() + " : " +Encoding.ASCII.GetString(data, 0, recv));

                messagesTexts += Remote.ToString() + " : " + Encoding.ASCII.GetString(data, 0, recv) + " " + DateTime.Now + "\n";

                break;
        }
    }

    private void Update()
    {
        threadClient.Join();

        if (img != null)
        {
            if (connected) { img.color = Color.green; }
            else { img.color = Color.red; }
        }

        if(textOnScreen != null)
        {
            textOnScreen.text = messagesTexts;
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
        Start();
    }

    void DisconnectSocket()
    {
        if (newSocket.Connected)
        {
            newSocket.Shutdown(SocketShutdown.Both);

            connected = false;
        }

        newSocket.Close();

        threadClient.Abort();
    }
}
