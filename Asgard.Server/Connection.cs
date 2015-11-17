using Asgard.Packets;
using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard
{
    public class Connection
    {
        static Connection()
        {
            Bootstrap.Init();
        }

        public Connection()
        {

        }

        public virtual NetPeer Peer {get;}


        public bool Send(Packet packet, NetConnection sendTo, int channel=0)
        {
            var msg = packet.SendMessage(this);
            NetSendResult result = Peer.SendMessage(msg, sendTo, packet.DeliveryMethod, channel);

            if (result == NetSendResult.Queued || result == NetSendResult.Sent)
                return true;
            else
                return false;
        }
    }

    public class ServerConnection : Connection
    {
        private NetServer _serverInstance;
        private volatile bool _running = false;
        private Thread _networkThread;

        private ConcurrentDictionary<ushort, Player> _Players = new ConcurrentDictionary<ushort, Player>();
        private ConcurrentDictionary<NetConnection, Player> _PlayerByConnection = new ConcurrentDictionary<NetConnection, Player>();


        public override NetPeer Peer
        {
            get
            {
                return _serverInstance as NetPeer;
            }         
        }

        public ServerConnection(int port)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Asgard.Server");
            config.Port = port;
            _serverInstance = new NetServer(config);

            RegisterPacketCallbacks();
        }

        private void RegisterPacketCallbacks()
        {
            PacketFactory.AddCallback<ConnectRequestPacket>(OnConnectRequest);
        }

        private void OnConnectRequest(ConnectRequestPacket packet)
        {
            if (packet.IsValid)
            {
                var responsePacket = new ConnectResponsePacket();
                responsePacket.Status = true;
                Send(responsePacket, packet.Connection);
            }
            else
            {
                //TODO
                packet.Connection.Disconnect("");
            }
        }

        #region Connection setup
        public bool Start()
        {
            if (_running)
            {
                return _running;
            }

            try
            {
                _serverInstance.Start();

                _networkThread = new Thread(_networkThreadFunc);
                _networkThread.IsBackground = true;
                _networkThread.Start();

                _running = true;
            }
            catch
            {

            }

            return _running;
        }

        public bool Stop()
        {
            if (!_running)
            {
                return _running;
            }

            try
            {
                _serverInstance.Shutdown("");
                _running = false;
            }
            catch
            {
            }

            return !_running;
        }

        private void _networkThreadFunc()
        {
            while(_running)
            {
                var message = _serverInstance.WaitMessage(100);
                if (message == null) continue;

                switch (message.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        //log
                        break;
                    case NetIncomingMessageType.Data:
                        var packet = Packet.Get(message);
                        packet.Connection = message.SenderConnection;
                        packet.ReceiveTime = message.ReceiveTime;

                        //handle player stale flags
                        var player = FindPlayerByConnection(message.SenderConnection);
                        if (player != null)
                        {
                            player.UpdateStaleState(message.ReceiveTime);
                        }

                        PacketFactory.RaiseCallbacks(packet);

                        break;
                    default:
                        //log
                        break;
                }
                _serverInstance.Recycle(message);
            }
        }
        #endregion

        #region player logic
        public Player AddPlayer(ushort id, NetConnection connection)
        {
            var player = FindPlayer(id);
            if (player != null)
            {
                var oldConnection = player.Connection;
                Player oldPlayer;
                _PlayerByConnection.TryRemove(oldConnection, out oldPlayer);

                player.ResetConnection(connection);
                _PlayerByConnection.TryAdd(connection, player);

            }
            else
            {
                player = new Player(connection, id);
                _Players.TryAdd(id, player);
                _PlayerByConnection.TryAdd(connection, player);
            }

            return player;

        }
        public void RemovePlayer(Player player)
        {
            var id = player.Id;
            Player oldPlayer;
            _Players.TryRemove(id, out oldPlayer);
            _PlayerByConnection.TryRemove(player.Connection, out oldPlayer);
            player.Connection.Disconnect("");
        }
        public Player FindPlayer(ushort id)
        {
            Player player;
            if (_Players.TryGetValue(id, out player))
            {
                return player;
            }

            return null;
        }
        public Player FindPlayerByConnection(NetConnection connection)
        {
            Player player = null;
            _PlayerByConnection.TryGetValue(connection, out player);
            return player;
        }
        public void CheckStalePlayers()
        {
            List<Player> players = _Players.Values.ToList();
            foreach(var player in players)
            {
                if (player.IsStale())
                {
                    RemovePlayer(player);
                }
            }
        }
        #endregion
    }
}
