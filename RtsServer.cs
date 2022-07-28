using System.Net;
using System.Net.Sockets;

namespace RtsServer;

internal class RtsServer
{
    public static int PlayersPerServer { get; set; }
    private static int Port { get; set; }
    public static Dictionary<int, Client> Clients;
    private static TcpListener _tcpListener;

    public delegate void PacketHandler(int sender, Packet packet);

    public static Dictionary<int, PacketHandler> packetHandler;

    public static void Start(int playersAmount, int port)
    {
        Clients = new Dictionary<int, Client>();
        PlayersPerServer = playersAmount;
        Port = port;
        Console.WriteLine("Starting Server");
        InitializeServerData();
        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        Console.WriteLine("Server ready");
    }

    private static void TcpConnectCallback(IAsyncResult result)
    {
        TcpClient client = _tcpListener.EndAcceptTcpClient(result);
        _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        Console.WriteLine($"Player connected from {client.Client.RemoteEndPoint}...");
        for (int i = 1; i <= PlayersPerServer; i++)
        {
            if (Clients[i].tcpClient.socket == null)
            {
                Clients[i].tcpClient.Connect(client);
                return;
            }
        }
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= PlayersPerServer; i++)
        {
            Clients.Add(i, new Client(i));
        }

        packetHandler = new Dictionary<int, PacketHandler>()
        {
            {(int) ClientPackets.welcomeReceived, ServerMessageHandler.WelcomeReceived},
            {(int) ClientPackets.addServer, ServerMessageHandler.AddServerToList},
            {(int) ClientPackets.serverlistRequested, ServerMessageHandler.PlayerRequestedServerlist},
            {(int) ClientPackets.playerJoined, ServerMessageHandler.PlayerJoinedServer},
            {(int) ClientPackets.playerLeft, ServerMessageHandler.PlayerLeftServer},
            {(int) ClientPackets.removeServer, ServerMessageHandler.RemoveServerFromList},
            {(int) ClientPackets.gameStarted, ServerMessageHandler.StartGame},
            {(int) ClientPackets.playerMovement, ServerMessageHandler.MoveUnit},
            {(int) ClientPackets.playerSpawned, ServerMessageHandler.PlayerCreatedUnit},
            {(int) ClientPackets.spawnBuilding, ServerMessageHandler.PlayerCreatedBuilding},
            {(int) ClientPackets.settings, ServerMessageHandler.SetSettings},
            {(int) ClientPackets.teamsettings, ServerMessageHandler.SetTeamColor},
            {(int) ClientPackets.building, ServerMessageHandler.PlayerIsBuilding},
            {(int) ClientPackets.readycheck, ServerMessageHandler.ReceiveReadyCheck},
            {(int) ClientPackets.destroyresource, ServerMessageHandler.DestroyResource},
        };
    }
}