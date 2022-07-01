using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;

namespace RtsServer;

public class Client
{
    public static int dataBufferSize = 1024;
    public int playerId;
    public Tcp tcpClient;
    public static Serverlist serverlist;

    public class Tcp
    {
        public TcpClient socket;
        private int id;
        private NetworkStream _networkStream;
        private byte[] receiveBuffer;
        private EventCreator eventMessage;

        public Tcp(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;
            _networkStream = socket.GetStream();
            receiveBuffer = new byte[dataBufferSize];
            eventMessage = new EventCreator();
            serverlist = new Serverlist();
            _networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            MessageSend.OnInitConnection(id);
        }

        public void SendData(EventCreator events)
        {
            try
            {
                if (socket != null)
                {
                    _networkStream.BeginWrite(events.TranslateToByte(), 0, events.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while sending Data");
                Console.WriteLine(e);
                throw;
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int messageByteLength = _networkStream.EndRead(result);
                if (messageByteLength <= 0)
                {
                    RtsServer.Clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[messageByteLength];
                Array.Copy(receiveBuffer, data, messageByteLength);
                HandleData(receiveBuffer, eventMessage);
                _networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while receiving Data");
                Console.WriteLine(ex);
                RtsServer.Clients[id].Disconnect();
            }
        }

        private void HandleData(byte[] _receiveBuffer, EventCreator eventCreator)
        {
            eventCreator.SetBytes(_receiveBuffer);
            string player_name;
            string server_name;
            string prefab_name;
            int server_id;
            int unit_id;

            Vector3 pos;
            Quaternion rota;

            string serverCall = eventCreator.ReadStringRange();
            int sender_id = eventCreator.ReadIntRange();
            int receiver_id;

            switch (serverCall)
            {
                case "welcome":
                    Console.WriteLine(eventCreator.ReadStringRange());
                    break;
                case "broadcast":
                    break;
                case "start_game":
                    int secondplayer_id = eventCreator.ReadIntRange();
                    MessageSend.StartGame(sender_id, secondplayer_id);
                    serverlist.GameStarted(sender_id);
                    break;
                case "addserver":
                    player_name = eventCreator.ReadStringRange();
                    server_name = eventCreator.ReadStringRange();
                    serverlist.CreateServer(sender_id, player_name, server_name);
                    break;
                case "removeserver":
                    serverlist.CloseServer(sender_id);
                    break;
                case "joinserver":
                    player_name = eventCreator.ReadStringRange();
                    server_id = eventCreator.ReadIntRange();
                    serverlist.JoinServer(server_id, sender_id, player_name);
                    MessageSend.SendUserJoinedLobbyMessage(server_id, sender_id, player_name);
                    break;
                case "leaveserver":
                    server_id = eventCreator.ReadIntRange();
                    serverlist.LeaveServer(server_id, sender_id);
                    break;
                case "receiveservers":
                    MessageSend.SendServerList(sender_id, serverlist.ReturnServerList());
                    break;
                case "unit_created":
                    server_id = eventCreator.ReadIntRange();
                    receiver_id = eventCreator.ReadIntRange();
                    var name = eventCreator.ReadStringRange();
                    unit_id =eventCreator.ReadIntRange();
                    var hp = eventCreator.ReadIntRange();
                    var dmg = eventCreator.ReadIntRange();
                    var ma = eventCreator.ReadIntRange();
                    var ra = eventCreator.ReadIntRange();
                    pos = eventCreator.ReadVector3Range();
                    rota = eventCreator.ReadQuaternionRange();
                    serverlist.ServerlistDictionary[server_id].PlayerDictionary[sender_id].AddUnit(receiver_id,unit_id,name,pos,rota,hp,dmg,ra,ma);
                    break;
                case "broadcast_pos":
                    receiver_id = eventCreator.ReadIntRange();
                    server_id = eventCreator.ReadIntRange();
                    unit_id =eventCreator.ReadIntRange();
                    pos = eventCreator.ReadVector3Range();
                    rota = eventCreator.ReadQuaternionRange();
                    serverlist.ServerlistDictionary[server_id].PlayerDictionary[sender_id]
                        .UpdateUnitPosition(receiver_id, unit_id,pos,rota);
                    Console.WriteLine("receiver: {0} server: {1} unit_id: {2} x: {3} y: {4} z: {5}",receiver_id,server_id,unit_id,pos.X,pos.Y,pos.Z);
                    break;
            }

            eventCreator.ClearBuffers();
        }

        public void Disconnect()
        {
            socket.Close();
            _networkStream = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public Client(int _clientID)
    {
        playerId = _clientID;
        tcpClient = new Tcp(playerId);
    }

    public void Disconnect()
    {
        Console.WriteLine("User Disconnected");
        tcpClient.Disconnect();
        //playerId = null;
    }
}