using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class ServerSockets : MonoBehaviour
{
    [SerializeField] socketType typeOfSocket;

    Socket server, client;

    Thread threadServer;

    byte[] data = new byte[1024];

    int recv;

    [SerializeField] bool onlyLocalHostIP = false;
    [SerializeField] int port;

    IPEndPoint ipep;

    [Header("UI Inspector Things")]
    [SerializeField] TMP_Text textTypeOfConnection;

    [SerializeField] TMP_Text textOnScreen;
    string messagesTexts;

    void Start()
    {
        // ======= Sockets =======

        switch (typeOfSocket)
        {
            case socketType.TCP:
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                textTypeOfConnection.text = "TCP";
                break;

            case socketType.UDP:
                server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                textTypeOfConnection.text = "UDP";
                break;
        }

        // ======= Addresses =======

        if (!onlyLocalHostIP)
        {
            ipep = new IPEndPoint(IPAddress.Any, port);
        }
        else
        {
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        }

        // ======= Binding =======

        server.Bind(ipep);

        // ======= Thread =======

        threadServer = new Thread(ServerThreadUpdate);
        threadServer.Start();
    }

    void ServerThreadUpdate()
    {
        switch (typeOfSocket) {
            case socketType.TCP:

                while (true)
                {
                    server.Listen(10);

                    DebugMessage("Waiting for a TCP Client...");

                    client = server.Accept();

                    Thread clientThread = new Thread(ClientThreadUpdate);
                    clientThread.Start(client);
                }
            case socketType.UDP:

                DebugMessage("Waiting for a UDP client...");

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);

                recv = server.ReceiveFrom(data, ref Remote);

                DebugMessage("Message received from " + Remote.ToString() + Encoding.ASCII.GetString(data, 0, recv));

                // == Send data to client ==
                string welcome_ = "You have connected to a UDP Server";
                data = Encoding.ASCII.GetBytes(welcome_);
                server.SendTo(data, data.Length, SocketFlags.None, Remote);

                // == Receive data from client ==
                while (true)
                {
                    data = new byte[1024];
                    recv = server.ReceiveFrom(data, ref Remote);

                    DebugMessage(Encoding.ASCII.GetString(data, 0, recv));

                    server.SendTo(data, recv, SocketFlags.None, Remote);
                }
        }
    }

    void ClientThreadUpdate(object clientS)
    {
        Socket client = (Socket)clientS;

        IPEndPoint clientP = (IPEndPoint)client.RemoteEndPoint;

        DebugMessage("Connected with " + clientP.Address.ToString() + " at port " + clientP.Port.ToString());

        // == Send data to client ==
        string welcome = "You have connected to a TCP Server";
        data = Encoding.ASCII.GetBytes(welcome);
        client.Send(data, data.Length, SocketFlags.None);

        // == Receive data from client ==
        while (true)
        {
            data = new byte[1024];
            recv = client.Receive(data);

            if (recv == 0)
            {
                break;
            }

            DebugMessage(Encoding.ASCII.GetString(data, 0, recv));

            client.Send(data, recv, SocketFlags.None);
        }

        DebugMessage("Disconnected from " + clientP.Address.ToString());

        client.Close();
    }

    void DisconnectSocket()
    {
        if (server.Connected)
        {
            server.Shutdown(SocketShutdown.Both);
        }

        server.Close();

        threadServer.Abort();
    }

    public void ResetConnection()
    {
        DisconnectSocket();
        Start();
    }

    public void ChangeSocketConnection()
    {
        if (typeOfSocket == socketType.TCP)
        {
            typeOfSocket = socketType.UDP;
            textTypeOfConnection.text = "UDP";
        }
        else
        {
            typeOfSocket = socketType.TCP;
            textTypeOfConnection.text = "TCP";
        }
    }

    enum socketType{
        UDP,
        TCP
    }

    private void OnApplicationQuit()
    {
        DisconnectSocket();
        Debug.Log("Closing Socket");
    }

    void DebugMessage(string message)
    {
        Debug.Log(message);
        messagesTexts = message + "\n" + messagesTexts;
    }

    private void Update()
    {
        if(textOnScreen != null) textOnScreen.text = messagesTexts;
    }

    public void CleanConsole()
    {
        messagesTexts = "";
    }
}
