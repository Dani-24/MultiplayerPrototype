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

        threadServer = new Thread(ThreadUpdate);
        threadServer.Start();
    }

    void ThreadUpdate()
    {
        switch (typeOfSocket) {
            case socketType.TCP:

                server.Listen(10);
                Debug.Log("Waiting for a TCP client...");

                client = server.Accept();

                IPEndPoint clientP = (IPEndPoint)client.RemoteEndPoint;

                Debug.Log("Connected with " + clientP.Address.ToString() + " at port " + clientP.Port.ToString());

                // == Send data to client ==
                string welcome = "Bombardeen Renfe Cercanias - Server";
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

                    Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
                    client.Send(data, recv, SocketFlags.None);
                }

                Console.WriteLine("Disconnected from " + clientP.Address.ToString());
                client.Close();

                break;
            case socketType.UDP:

                Debug.Log("Waiting for a UDP client...");

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);

                recv = server.ReceiveFrom(data, ref Remote);

                Debug.Log("Message received from " + Remote.ToString());
                Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

                // == Send data to client ==
                string welcome_ = "Bombardeen Renfe Cercanias - Server";
                data = Encoding.ASCII.GetBytes(welcome_);
                server.SendTo(data, data.Length, SocketFlags.None, Remote);

                // == Receive data from client ==
                while (true)
                {
                    data = new byte[1024];
                    recv = server.ReceiveFrom(data, ref Remote);

                    Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
                    server.SendTo(data, recv, SocketFlags.None, Remote);
                }
        }
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
}
