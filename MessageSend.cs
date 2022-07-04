using System.Numerics;

namespace RtsServer;

public class MessageSend
{
    public static void OnInitConnection(int receiverClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write("You were Connected");
            packet.Write(receiverClient);
            SendTcpMessage(receiverClient, packet);
        }
        Console.WriteLine("Sending Welcome package");
    }
    
    public static void ServerTransfer(int receiverClient, int oldServerOwner)
    {
        using (Packet packet = new Packet((int) ServerPackets.serverTransfered))
        {
            packet.Write(oldServerOwner);
            SendTcpMessage(receiverClient, packet);
        }
        Console.WriteLine("Sending server transfer request");
    }

    public static void StartGame(int creator_id, int receiverClient)
    {
        using (Packet packet_player1 = new Packet((int) ServerPackets.gameStarted))
        {
            packet_player1.Write(receiverClient);
            SendTcpMessage(creator_id, packet_player1);
        }
        using (Packet packet_player2 = new Packet((int) ServerPackets.gameStarted))
        {
            packet_player2.Write(creator_id);
            SendTcpMessage(receiverClient, packet_player2);
        }
        Console.WriteLine("Broadcasting the start of game {0}", creator_id);
    }

    public static void PlayerLeaving(int receiverClient)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerLeft))
        {
            SendTcpMessage(receiverClient, packet);
        }
    }

    public static void SendServerList(int receiverClient, Dictionary<int, Serverlist> serverlist)
    {
        using (Packet packet = new Packet((int) ServerPackets.serverlistRequested))
        {
            packet.Write(serverlist.Count);
            foreach (var kvp in serverlist)
            {
                Console.WriteLine("Sending Server_id = {0} to receiver_id: {1}", kvp.Key, receiverClient);
                var server = kvp.Value;
                packet.Write(server.player1_id);
                packet.Write(server.player1_name);
                packet.Write(server.player2_id);
                packet.Write(server.player2_name);
                packet.Write(server.server_name);
                packet.Write(server.server_full);
                packet.Write(server.currently_ingame);
            }
            SendTcpMessage(receiverClient, packet);
        }
        Console.WriteLine("Serverlist was requested and send");
    }

    public static void SendUserJoinedLobbyMessage(int receiverClient, int joiningplayer_id, string joiningplayer_name)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerJoined))
        {
            packet.Write(joiningplayer_id);
            packet.Write(joiningplayer_name);
            SendTcpMessage(receiverClient, packet);
        }
    }

    private static void SendTcpMessageToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= RtsServer.PlayersPerServer; i++)
        {
            RtsServer.Clients[i].tcpClient.SendData(packet);
        }
    }

    private static void SendTcpMessageToAll(int excludedClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= RtsServer.PlayersPerServer; i++)
        {
            if (i != excludedClient)
                RtsServer.Clients[i].tcpClient.SendData(packet);
        }
    }

    private static void SendTcpMessage(int receiverClient, Packet packet)
    {
        packet.WriteLength();
        RtsServer.Clients[receiverClient].tcpClient.SendData(packet);
    }

    public static void SendUnitCreated(int receiverId, UnitData unitData)
    {
        using (Packet packet = new Packet((int) ServerPackets.spawnPlayer))
        {
            packet.Write(unitData.id);
            packet.Write(unitData.prefabname);
            packet.Write(unitData.position);
            packet.Write(unitData.rotation);
            SendTcpMessage(receiverId, packet);
        }
        Console.WriteLine("Unit created and now broadcasting to other player");
    }

    public static void SendUnitPos(int receiverId, int unitId, Vector3 pos)
    {
        Console.WriteLine("Sending unit pos");
        using (Packet packet = new Packet((int) ServerPackets.playerPosition))
        {
            packet.Write(unitId);
            packet.Write(pos);
            SendTcpMessage(receiverId, packet);
        }
    }
}