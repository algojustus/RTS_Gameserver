using System.Diagnostics;
using System.Numerics;

namespace RtsServer;

public class ServerMessageHandler
{
    public static void WelcomeReceived(int sender, Packet packet)
    {
        string text = packet.ReadString();
        int clientID = packet.ReadInt();
        Console.WriteLine(sender + " " + text + " his client_id: " + clientID);
    }

    public static void AddServerToList(int sender, Packet packet)
    {
        string player_name = packet.ReadString();
        string server_name = packet.ReadString();
        Client.serverlist.CreateServer(sender, player_name, server_name);
        Console.WriteLine("ServerAdded");
    }

    public static void StartGame(int sender, Packet packet)
    {
        int secondplayer_id = packet.ReadInt();
        MessageSend.StartGame(sender, secondplayer_id);
        Client.serverlist.GameStarted(sender);
        Console.WriteLine("GameStarted");
    }

    public static void RemoveServerFromList(int sender, Packet packet)
    {
        Client.serverlist.CloseServer(sender);
        Console.WriteLine("ServerRemoved");
    }

    public static void PlayerJoinedServer(int sender, Packet packet)
    {
        string player_name = packet.ReadString();
        int server_id = packet.ReadInt();
        Client.serverlist.JoinServer(server_id, sender, player_name);
        MessageSend.SendUserJoinedLobbyMessage(server_id, sender, player_name);
        Console.WriteLine("JoinedServer");
    }

    public static void PlayerLeftServer(int sender, Packet packet)
    {
        int server_id = packet.ReadInt();
        Client.serverlist.LeaveServer(server_id, sender);
        Console.WriteLine("LeftServer");
    }

    public static void PlayerCreatedUnit(int sender, Packet packet)
    {
        int server_id = packet.ReadInt();
        int receiver_id = packet.ReadInt();
        var name = packet.ReadString();
        int unit_id = packet.ReadInt();
        var hp = packet.ReadInt();
        var dmg = packet.ReadInt();
        var ma = packet.ReadInt();
        var ra = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();
        Quaternion rota = packet.ReadQuaternion();
        Client.serverlist.ServerlistDictionary[server_id].PlayerDictionary[sender]
            .AddUnit(receiver_id, unit_id, name, pos, rota, hp, dmg, ra, ma);
        Console.WriteLine("UnitCreated");
    }

    public static void PlayerCreatedBuilding(int sender, Packet packet)
    {
        int server_id = packet.ReadInt();
        int receiver_id = packet.ReadInt();
        int building_id = packet.ReadInt();
        var name = packet.ReadString();
        Vector3 pos = packet.ReadVector3();
        Quaternion rota = packet.ReadQuaternion();
        var hp = packet.ReadInt();
        var dmg = packet.ReadInt();
        var ma = packet.ReadInt();
        var ra = packet.ReadInt();
        Client.serverlist.ServerlistDictionary[server_id].PlayerDictionary[sender]
            .AddBuilding(receiver_id, building_id, name, pos, rota, hp, ra, ma);
        Console.WriteLine("BuildingCreated");
    }

    public static void MoveUnit(int sender, Packet packet)
    {
        int receiver_id = packet.ReadInt();
        int server_id = packet.ReadInt();
        int unit_id = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();
        Client.serverlist.ServerlistDictionary[server_id].PlayerDictionary[sender]
            .UpdateUnitPosition(receiver_id, unit_id, pos);
        Console.WriteLine("receiver: {0} server: {1} unit_id: {2} x: {3} y: {4} z: {5}", receiver_id, server_id,
            unit_id, pos.X, pos.Y, pos.Z);
    }

    public static void PlayerRequestedServerlist(int sender, Packet packet)
    {
        MessageSend.SendServerList(sender, Client.serverlist.ReturnServerList());
        Console.WriteLine("RequestedServerlist");
    }

    public static void SetTeamColor(int sender, Packet packet)
    {
        int server_id = packet.ReadInt();
        int player = packet.ReadInt();
        //Client.serverlist.ServerlistDictionary[server_id].PlayerDictionary[sender].color = player;
        int[] playerlist = new int[Client.serverlist.ServerlistDictionary[server_id].maxPlayers];
        playerlist = Client.serverlist.ServerlistDictionary[server_id].FillPlayerList();
        foreach (var id in playerlist)
        {
            if (id != sender && id != 0)
                MessageSend.SetTeamColor(id, player);
        }
    }

    public static void SetSettings(int sender, Packet packet)
    {
        int server_id = packet.ReadInt();
        int max_player = packet.ReadInt();
        int max_villagers = packet.ReadInt();
        int startResource = packet.ReadInt();

        Client.serverlist.ServerlistDictionary[server_id].maxPlayers = max_player;
        Client.serverlist.ServerlistDictionary[server_id].maxUnits = max_villagers;
        int[] playerlist = new int[Client.serverlist.ServerlistDictionary[server_id].maxPlayers];
        playerlist = Client.serverlist.ServerlistDictionary[server_id].FillPlayerList();
        foreach (var id in playerlist)
        {
            if (id != sender && id != 0)
                MessageSend.SetSettingsForAll(id, max_player, max_villagers, startResource);
        }
    }

    public static void PlayerIsBuilding(int sender, Packet packet)
    {
        int receiver_id = packet.ReadInt();
        int server_id = packet.ReadInt();
        int building_id = packet.ReadInt();
        int multiplier = packet.ReadInt();
        bool initialized = packet.ReadBool();
        bool finished = packet.ReadBool();
        MessageSend.BuildingIsBuild(sender,receiver_id,building_id,multiplier,initialized,finished);
    }
    
    public static void ReceiveReadyCheck(int sender, Packet packet)
    {
        int server_id = packet.ReadInt();
        int player = packet.ReadInt();
        bool ready = packet.ReadBool();

        int[] playerlist = new int[Client.serverlist.ServerlistDictionary[server_id].maxPlayers];
        playerlist = Client.serverlist.ServerlistDictionary[server_id].FillPlayerList();
        foreach (var id in playerlist)
        {
            if (id != sender && id != 0)
                //Console.WriteLine("{0}{1}{2}",player,receiver_id,ready);
                MessageSend.ReadyCheck(player,id,ready);
        }
    }
}