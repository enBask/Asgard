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
            config.AutoFlushSendQueue = true;
            config.UseMessageRecycling = true;
            config.SimulatedLoss = 0.02f;
            config.SimulatedMinimumLatency = 0.05f;
            config.SimulatedRandomLatency = 0.05f;

            _serverInstance = new NetServer(config);

            RegisterPacketCallbacks();
        }
        #endregion

        public void Tick( float tick )
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
