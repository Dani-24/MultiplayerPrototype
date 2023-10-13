using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;

public class ServerSockets : MonoBehaviour
{
    [SerializeField] socketType typeOfSocket;

    Socket newSocket, client;

    Thread threadServer;

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

        threadServer = new Thread(ThreadUpdate);
        threadServer.Start();
    }

    void ThreadUpdate()
    {
        switch (typeOfSocket) {
            case socketType.TCP:

                Debug.Log("Waiting for a client...");

                newSocket.Listen(10);

                client = newSocket.Accept();

                IPEndPoint clientP = (IPEndPoint)client.RemoteEndPoint;

                Debug.Log("Connected with " + clientP.Address.ToString() + " at port " + clientP.Port.ToString());

                string welcome = "Bombardeen Renfe Cercanias";
                data = Encoding.ASCII.GetBytes(welcome);

                client.Send(data, data.Length, SocketFlags.None);

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

                Debug.Log("Waiting for a client...");

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);

                recv = newSocket.ReceiveFrom(data, ref Remote);

                Debug.Log("Message received from " + Remote.ToString());
                Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

                string welcome_ = "Bombardeen Renfe Cercanias";
                data = Encoding.ASCII.GetBytes(welcome_);

                newSocket.SendTo(data, data.Length, SocketFlags.None, Remote);

                while(true)
                {
                    data = new byte[1024];
                    recv = newSocket.ReceiveFrom(data, ref Remote);

                    Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
                    newSocket.SendTo(data, recv, SocketFlags.None, Remote);
                }
                //break;
        }

        // Disconnect 

        if(newSocket.Connected)
        {
            newSocket.Shutdown(SocketShutdown.Both);
        }

        newSocket.Close();

    }

    enum socketType{
        UDP,
        TCP
    }
}
