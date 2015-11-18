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
            NetSendResult result = Peer.SendMessage(msg, sendTo, packet.Method, channel);

            if (result == NetSendResult.Queued || result == NetSendResult.Sent)
                return true;
            else
                return false;
        }
    }

    public class ServerConnection : Connection
    {
        #region delegates
        public delegate void OnConnectedHandler(NetConnection connection);
        public delegate void OnDisconnectHandler(NetConnection connection);
        #endregion

        #region public events
        public event OnConnectedHandler OnConnection;
        public event OnDisconnectHandler OnDisconnect;
        #endregion

        #region private vars
        private NetServer _serverInstance;
        private volatile bool _running = false;
        private Thread _networkThread;

        private ConcurrentDictionary<ushort, Player> _Players = new ConcurrentDictionary<ushort, Player>();
        private ConcurrentDictionary<NetConnection, Player> _PlayerByConnection = new ConcurrentDictionary<NetConnection, Player>();
        #endregion

        #region Properties
        public override NetPeer Peer
        {
            get
            {
                return _serverInstance as NetPeer;
            }         
        }
        #endregion

        #region ctors
        public ServerConnection(int port, int maxconnections)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Asgard.Server");
            config.Port = port;
            config.MaximumConnections = maxconnections;

            config.AcceptIncomingConnections = true;
            config.AutoExpandMTU = true;
            config.AutoFlushSendQueue = true;
            config.UseMessageRecycling = true;

            _serverInstance = new NetServer(config);

            RegisterPacketCallbacks();
        }
        #endregion


        private void OnRaiseConnectedEvent(NetConnection connection)
        {
            if (OnConnection == null) return;


            List<Exception> exceptions = null;

            var handlers = OnConnection.GetInvocationList();
            foreach(OnConnectedHandler handler in handlers)
            {
                try
                {
                    handler(connection);
                }
                catch(Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    //TODO: log error maybe fail after all called.
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException("handler error", exceptions);
            }

            return;
        }

        private void OnRaiseDisconnectedEvent(NetConnection connection)
        {
            if (OnDisconnect == null) return;

            List<Exception> exceptions = null;

            var handlers = OnDisconnect.GetInvocationList();
            foreach (OnDisconnectHandler handler in handlers)
            {
                try
                {
                    handler(connection);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    //TODO: log error maybe fail after all called.
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException("handler error", exceptions);
            }

            //remove player tied to connection.
            var player = FindPlayerByConnection(connection);
            if (player != null)
            {
                RemovePlayer(player);
            }

            return;
        }

        private void RegisterPacketCallbacks()
        {
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
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
                        switch(status)
                        {
                            case NetConnectionStatus.Connected:

                                break;
                            case NetConnectionStatus.Disconnected:
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        var packet = Packet.Get(message);
                        packet.Connection = message.SenderConnection;
                        packet.ReceiveTime = message.ReceiveTime;
                       
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
        #endregion
    }
}
