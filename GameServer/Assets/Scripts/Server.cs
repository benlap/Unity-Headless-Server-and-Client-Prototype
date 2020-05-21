using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);



        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);



        Debug.Log($"Server started on {Port}. IP: {GetServerIP()}");
    }

    private static string GetServerIP()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);



        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                return;
            }
        }

        Console.Write($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null)
                {
                    clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndPont, Packet packet)
    {
        try
        {
            if (clientEndPont != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPont, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPont} via UDP: {ex}");
        }
    }

    private static void InitializeServerData()
    {
        for (int i = 0; i < MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            {(int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
            {(int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem }
        };

        Debug.Log("Initialized packets.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}