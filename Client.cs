using System.Net.Sockets;

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
        private Packet eventPacket;
        private NetworkStream _networkStream;
        private int id;
        private byte[] receiveBuffer;
        

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
            eventPacket = new Packet();
            serverlist = new Serverlist();
            _networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            MessageSend.OnInitConnection(id);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    _networkStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
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
                eventPacket.Reset(HandleData(data));
                _networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while receiving Data");
                Console.WriteLine(ex);
                RtsServer.Clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            eventPacket.SetBytes(_data);

            if (eventPacket.UnreadLength() >= 4)
            {
                _packetLength = eventPacket.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= eventPacket.UnreadLength())
            {
                byte[] _packetBytes = eventPacket.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        RtsServer.packetHandler[_packetId](id, _packet);
                    }
                });

                _packetLength = 0;
                if (eventPacket.UnreadLength() >= 4)
                {
                    _packetLength = eventPacket.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
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
    }
}