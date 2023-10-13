using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;
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

    void Start()
    {
        // ======= Sockets =======

        switch (typeOfSocket)
        {
            case socketType.TCP:
                newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                break;

            case socketType.UDP:
                newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
        switch (typeOfSocket)
        {
            case socketType.TCP:

                try
                {
                    newSocket.Connect(ipep);
                }
                catch (SocketException e)
                {
                    Debug.Log("Failed to connect to server: " + e.ToString());
                    return;
                }

                recv = newSocket.Receive(data);
                string message = Encoding.ASCII.GetString(data, 0, recv);
                Debug.Log("Data received: " + message);

                break;
            case socketType.UDP:

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);

                string welcome = "UDP Client Bombardeando Renfe";
                data = Encoding.ASCII.GetBytes(welcome);
                newSocket.SendTo(data, data.Length, SocketFlags.None, ipep);

                data = new byte[1024];
                recv = newSocket.ReceiveFrom(data, ref Remote);

                Debug.Log("Message received from "+ Remote.ToString() + " : " +Encoding.ASCII.GetString(data, 0, recv));

                break;
        }

        // Disconnect 

        if (newSocket.Connected)
        {
            newSocket.Shutdown(SocketShutdown.Both);
        }

        newSocket.Close();

    }

    enum socketType
    {
        UDP,
        TCP
    }
}
