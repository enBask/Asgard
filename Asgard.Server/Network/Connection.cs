using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Manager;
using Asgard.Core.System;

namespace Asgard
{
    public class BifrostServer : Connection, ISystem
    {
        #region delegates
        public delegate void OnConnectedHandler(NetNode connection);
        public delegate void OnDisconnectHandler(NetNode connection);
        #endregion

        #region public events
        public event OnConnectedHandler OnConnection;
        public event OnDisconnectHandler OnDisconnect;
        #endregion

        #region private vars
        private NetServer _serverInstance;
        private volatile bool _running = false;
        private Thread _networkThread;
        #endregion

        #region Properties
        public override NetPeer Peer
        {
            get
            {
                return _serverInstance as NetPeer;
            }         
        }

        public AsgardBase Base { get; set; }

        private float _lastNetCheck;
        private NetStats _stats;
        public NetStats GetStats()
        {
            if (Peer == null || Peer.Statistics == null)
                return null;

            //if (_stats == null)
            {
                _lastNetCheck = (float)Lidgren.Network.NetTime.Now;
                _stats = new NetStats();
            }

            var diff = (float)Lidgren.Network.NetTime.Now - _lastNetCheck;

            _stats.BytesInPerSec = Peer.Statistics.ReceivedBytes;// / diff;
            _stats.BytesOutPerSec = Peer.Statistics.SentBytes;// / diff;

            return _stats;
        }


        public EntityManager EntityManager
        {
            get; internal set;
        }
        #endregion

        #region ctors
        public BifrostServer(int port, int maxconnections)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Asgard");
            config.Port = port;
            config.MaximumConnections = maxconnections;

            config.AcceptIncomingConnections = true;
            config.AutoExpandMTU = true;
            config.UseMessageRecycling = true;

//              config.SimulatedLoss = 0.01f;
//              config.SimulatedMinimumLatency = 0.05f;
//              config.SimulatedRandomLatency = 0.05f;

            _serverInstance = new NetServer(config);

            RegisterPacketCallbacks();
        }
        #endregion

        public void Tick( double tick )
        {

        }


        private void OnRaiseConnectedEvent(NetConnection connection)
        {
            if (OnConnection == null) return;


            List<Exception> exceptions = null;

            var handlers = OnConnection.GetInvocationList();
            foreach(OnConnectedHandler handler in handlers)
            {
                try
                {
                    handler((NetNode)connection);
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
                    handler((NetNode)connection);
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
// 
//                 _networkThread = new Thread(_networkThreadFunc);
//                 _networkThread.IsBackground = true;
//                 _networkThread.Start();

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

        internal void pumpNetwork()
        {
            while(true)
            {
                var message = _serverInstance.WaitMessage(1);
                if (message == null) break;

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
                                OnRaiseConnectedEvent(message.SenderConnection);
                                break;
                            case NetConnectionStatus.Disconnected:
                                OnRaiseDisconnectedEvent(message.SenderConnection);
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        var packet = Packet.Get(message);
                        packet.Connection = (NetNode)message.SenderConnection;
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

        /// Assuming that excludeGroup is small
        public void Send(Packet packet, IList<NetNode> sendToList, IList<NetNode> excludeGroup, int channel = 0)
        {
            var msg = packet.SendMessage(this);

            var group = sendToList.Except(excludeGroup).Cast<NetConnection>().ToList();

            if (group.Count == 0) return;
            Peer.SendMessage(msg, group, (Lidgren.Network.NetDeliveryMethod)packet.Method, channel);
        }

        public void Send(Packet packet, IList<NetNode> sendToList, NetNode excludeNode = null, int channel = 0)
        {
            var msg = packet.SendMessage(this);

            List<NetConnection> group = new List<NetConnection>();

            foreach (var node in sendToList)
            {
                if (excludeNode == node)
                    continue;

                group.Add(node);
            }

            if (group.Count == 0) return;
            Peer.SendMessage(msg, group, (Lidgren.Network.NetDeliveryMethod)packet.Method, channel);
        }
        #endregion
    }
}
