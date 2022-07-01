using System.Numerics;

namespace RtsServer;

public class MessageSend
{
    public static void OnInitConnection(int _receiverClient)
    {
        Console.WriteLine("Sending Welcome package");
        EventCreator events = new EventCreator();
        events.Write(_receiverClient);
        events.Write("You were connected");
        SendTcpMessage(_receiverClient, events);
    }

    public static void ServerTransfer(int _receiverClient, int oldServerOwner)
    {
        Console.WriteLine("Sending server transfer request");
        EventCreator events = new EventCreator();
        events.Write("server_transfer");
        events.Write(oldServerOwner);
        SendTcpMessage(_receiverClient, events);
    }

    public static void StartGame(int creator_id, int receiverClient)
    {
        Console.WriteLine("Broadcasting the start of game {0}", creator_id);
        EventCreator events = new EventCreator();
        EventCreator events2 = new EventCreator();
        events.Write("start_match");
        events2.Write("start_match");
        events.Write(receiverClient);
        events2.Write(creator_id);
        SendTcpMessage(creator_id, events);
        SendTcpMessage(receiverClient, events2);
    }

    public static void PlayerLeaving(int _receiverClient)
    {
        EventCreator events = new EventCreator();
        events.Write("playerleft");
        SendTcpMessage(_receiverClient, events);
    }

    public static void SendServerList(int _receiverClient, Dictionary<int, Serverlist> serverlist)
    {
        Console.WriteLine("Serverlist was requested and send");
        EventCreator events = new EventCreator();
        events.Write("receive_serverlist");
        events.Write(serverlist.Count);

        foreach (var kvp in serverlist)
        {
            Console.WriteLine("Sending Server_id = {0} to receiver_id: {1}", kvp.Key, _receiverClient);
            var server = kvp.Value;
            events.Write(server.player1_id);
            events.Write(server.player1_name);
            events.Write(server.player2_id);
            events.Write(server.player2_name);
            events.Write(server.server_name);
            events.Write(server.server_full);
            events.Write(server.currently_ingame);
        }

        SendTcpMessage(_receiverClient, events);
    }

    public static void SendUserJoinedLobbyMessage(int receiverClient, int joiningplayer_id, string joiningplayer_name)
    {
        EventCreator events = new EventCreator();
        events.Write("playerjoined");
        events.Write(joiningplayer_id);
        events.Write(joiningplayer_name);
        SendTcpMessage(receiverClient, events);
    }

    private static void SendTcpMessageToAll(EventCreator _events)
    {
        for (int i = 1; i <= RtsServer.PlayersPerServer; i++)
        {
            RtsServer.Clients[i].tcpClient.SendData(_events);
        }
    }

    private static void SendTcpMessageToAll(int _excludedClient, EventCreator _events)
    {
        for (int i = 1; i <= RtsServer.PlayersPerServer; i++)
        {
            if (i != _excludedClient)
                RtsServer.Clients[i].tcpClient.SendData(_events);
        }
    }

    private static void SendTcpMessage(int _receiverClient, EventCreator _events)
    {
        RtsServer.Clients[_receiverClient].tcpClient.SendData(_events);
    }

    public static void SendUnitCreated(int receiverId, UnitData unitData)
    {
        Console.WriteLine("Unit created and now broadcasting to other player");
        EventCreator events = new EventCreator();
        events.Write("unit_created");
        events.Write(unitData.id);
        events.Write(unitData.prefabname);
        events.Write(unitData.position);
        events.Write(unitData.rotation);
        SendTcpMessage(receiverId, events);
    }

    public static void SendUnitPos(int receiverId, int unitId, Vector3 pos, Quaternion rota)
    {
        EventCreator events = new EventCreator();
        events.Write("update_unit_pos");
        events.Write(unitId);
        events.Write(pos);
        events.Write(rota);
        SendTcpMessage(receiverId, events);
    }
}