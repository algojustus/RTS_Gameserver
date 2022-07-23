using System.Diagnostics;
using System.Net.Sockets;

namespace RtsServer;

public class Serverlist
{
    public Dictionary<int, Serverlist> ServerlistDictionary;
    public Dictionary<int, PlayerData> PlayerDictionary;
    public int player1_id;
    public int player2_id;
    public int player3_id;
    public int player4_id;
    public int[] playerlist;
    public string server_name = "";
    public string player1_name = "";
    public string player2_name = "";
    public bool currently_ingame;
    public bool server_full;
    public int maxPlayers = 2;
    public int maxUnits;
    public Serverlist(int creator_id, string creator_name, string _server_name)
    {
        server_full = false;
        player1_id = creator_id;
        player1_name = creator_name;
        server_name = _server_name;
    }

    public Serverlist()
    {
        ServerlistDictionary = new Dictionary<int, Serverlist>();
        PlayerDictionary = new Dictionary<int, PlayerData>();
    }
    
    public int[] FillPlayerList()
    {
        playerlist = new int[maxPlayers];
        for (int i = 1; i <= maxPlayers; i++)
        {
            playerlist[i - 1] = GetPlayerByNumber(i);
        }
        return playerlist;
    }

    private int GetPlayerByNumber(int i)
    {
        int currentPlayerId = 0;
        switch (i)
        {
            case 1:
                currentPlayerId = player1_id;
                break;
            case 2: 
                currentPlayerId = player2_id;
                break;
            case 3: 
                currentPlayerId = player3_id;
                break;
            case 4: 
                currentPlayerId = player4_id;
                break;
        }
        return currentPlayerId;
    }
    public void JoinServer(int creator_id, int joining_id, string joining_name)
    {
        var server = ServerlistDictionary[creator_id];
        Console.WriteLine("Player {0}:{1} joined lobby of {2}:{3}"
            , joining_id
            , joining_name
            , creator_id
            , server.player1_name);
        server.player2_id = joining_id;
        server.player2_name = joining_name;
        server.server_full = true;
    }

    public void LeaveServer(int server_id, int leavingplayer_id)
    {
        var server = ServerlistDictionary[server_id];
        Console.WriteLine("Player {0}:{1} left lobby of {2}:{3}"
            , leavingplayer_id
            , server.player2_name
            , server_id
            , server.player1_name);
        server.player2_id = default;
        server.player2_name = "";
        server.server_full = false;
        MessageSend.PlayerLeaving(server_id);
    }

    public void GameStarted(int creator_id)
    {
        var server = ServerlistDictionary[creator_id];
        server.PlayerDictionary = new Dictionary<int, PlayerData>();
        Console.WriteLine("Game from {0}:{1} started with player {2}:{3}"
            , server.player1_id
            , server.player1_name
            , server.player2_id
            , server.player2_name);
        server.currently_ingame = true;
        server.PlayerDictionary.Add(1, new PlayerData(server.player1_id,server.player1_name,1));
        server.PlayerDictionary.Add(2, new PlayerData(server.player2_id,server.player2_name,2));
    }

    public (string player1_name, string player2_name) GetBothPlayernames(int creator_id)
    {
        var server = ServerlistDictionary[creator_id];
        player1_name = server.player1_name;
        player2_name = server.player2_name;
        return (player1_name, player2_name);
    }

    public void CreateServer(int creator_id, string creator_name, string server_name)
    {
        Serverlist server = new Serverlist(creator_id, creator_name, server_name);

        if (!ServerlistDictionary.ContainsKey(creator_id))
        {
            Console.WriteLine("Server Added for {0}:{1} with Servername: {2}", creator_id, creator_name, server_name);
            ServerlistDictionary.Add(creator_id, server);
            return;
        }

        Console.WriteLine("Server exists, re-initializing it");
        ServerlistDictionary.Remove(creator_id);
        ServerlistDictionary.Add(creator_id, server);
    }

    public void CloseServer(int creator_id)
    {
        var server = ServerlistDictionary[creator_id];
        if (!ServerlistDictionary.ContainsKey(creator_id))
            return;

        if (server.player2_id == 0)
        {
            Console.WriteLine("Removing Server ID: {0} now", creator_id);
            ServerlistDictionary.Remove(creator_id);
            return;
        }

        Console.WriteLine("Transfering Lobby ID: {0} to ID: {1}", creator_id, server.player2_id);
        MessageSend.ServerTransfer(server.player2_id, server.player1_id);
        server.player1_id = player2_id;
        server.player1_name = player2_name;
        server.player2_id = default;
        server.player2_name = "";
        server.server_full = false;
        ServerlistDictionary.Add(server.player1_id, server);
        ServerlistDictionary.Remove(creator_id);
    }

    public Dictionary<int, Serverlist> ReturnServerList()
    {
        return ServerlistDictionary;
    }

    public void PrintServerList()
    {
        foreach (var kvp in ServerlistDictionary)
        {
            Console.WriteLine("Server_ID = {0}", kvp.Key);
        }
    }
}