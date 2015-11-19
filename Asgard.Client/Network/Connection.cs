﻿using Asgard.Core.Network;
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

namespace Asgard
{
    public class BifrostClient : Connection
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
        private NetClient _clientInstance;
        private volatile bool _running = false;
        private Thread _networkThread;

        private IPEndPoint _endPoint;
        #endregion

        public override NetPeer Peer
        {
            get
            {
                return _clientInstance as NetPeer;
            }
        }

        public BifrostClient(string host, int port)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Asgard");
            config.AcceptIncomingConnections = false;
            config.AutoExpandMTU = true;
            config.AutoFlushSendQueue = true;
            config.UseMessageRecycling = true;


            IPAddress address = NetUtility.Resolve(host);
            _endPoint = new IPEndPoint(address, port);
            _clientInstance = new NetClient(config);
        }


        private void OnRaiseConnectedEvent(NetConnection connection)
        {
            if (OnConnection == null) return;


            List<Exception> exceptions = null;

            var handlers = OnConnection.GetInvocationList();
            foreach (OnConnectedHandler handler in handlers)
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

        public void Send(Packet packet, int channel = 0)
        {
            var conn = _clientInstance.ServerConnection;
            Send(packet, (NetNode)conn, channel);
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
                _clientInstance.Start();
                _clientInstance.Connect(_endPoint);

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
                _clientInstance.Shutdown("");
                _running = false;
            }
            catch
            {
            }

            return !_running;
        }

        private void _networkThreadFunc()
        {
            while (_running)
            {
                var message = _clientInstance.WaitMessage(100);
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
                        switch (status)
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
                _clientInstance.Recycle(message);
            }
        }
        #endregion

    }

}