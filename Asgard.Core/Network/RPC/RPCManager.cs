using Artemis;
using Asgard.Core.Network.Packets;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network.RPC
{
    public static class RPCManager
    {
        
        static Dictionary<string, Dictionary<long, Action<List<object>>>> _rpcLookup;

        static Connection _connection;
        static RPCManager()
        {
            _rpcLookup = new Dictionary<string, Dictionary<long, Action<List<object>>>>();
        }

        public static void SetConnection(Connection conn)
        {
            _connection = conn;
        }

        internal static void _Call(string name, long entityId, List<object> parms)
        {
            Dictionary<long, Action<List<object>>> routes = null;
            Action<List<object>> callback = null;
            if (_rpcLookup.TryGetValue(name, out routes))
            {
                if (routes.TryGetValue(entityId, out callback))
                {
                    callback(parms);
                }
            }
        }

        #region Call methods
        public static void CallSingle(string name, Entity entity = null, NetNode node = null)
        {
            if (node == null)
            {
                node = (NetNode)_connection.Peer.Connections[0];
            }

            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.EntityId = entity != null ? (uint)entity.UniqueId : 0 ;
            packet.Parameters = new List<object>();
            _connection.Send(packet, node, 10);
        }

        public static void Call<T>(string name, Entity entity, T p1, NetNode node=null)
        {
            if (node == null)
            {
                node = (NetNode)_connection.Peer.Connections[0];
            }

            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.EntityId = entity != null ? (uint)entity.UniqueId : 0;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            _connection.Send(packet, node, 10);
        }
        public static void Call<T1, T2>(string name, Entity entity, T1 p1, T2 p2, NetNode node = null)
        {
            if (node == null)
            {
                node = (NetNode)_connection.Peer.Connections[0];
            }

            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.EntityId = entity != null ? (uint)entity.UniqueId : 0;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            packet.Parameters.Add(p2);
            _connection.Send(packet, node, 10);
        }
        public static void Call<T1, T2, T3>(string name, Entity entity, T1 p1, T2 p2, T3 p3, NetNode node = null)
        {
            if (node == null)
            {
                node = (NetNode)_connection.Peer.Connections[0];
            }

            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.EntityId = entity != null ? (uint)entity.UniqueId : 0;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            packet.Parameters.Add(p2);
            packet.Parameters.Add(p3);
            _connection.Send(packet, node, 10);
        }
        public static void Call<T1, T2, T3, T4>(string name, Entity entity, T1 p1, T2 p2, T3 p3, T4 p4, NetNode node = null)
        {
            if (node == null)
            {
                node = (NetNode)_connection.Peer.Connections[0];
            }


            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.EntityId = entity != null ? (uint)entity.UniqueId : 0;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            packet.Parameters.Add(p2);
            packet.Parameters.Add(p3);
            packet.Parameters.Add(p4);
            _connection.Send(packet, node, 10);
        }
        #endregion

        #region Register Methods
        public static Action<NetNode> Register(string name, Entity entity, Action callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut();
            });

            long eid = 0;
            if (entity != null)
                eid = entity.UniqueId;

            Dictionary<long, Action<List<object>>> routes;
            if (!_rpcLookup.TryGetValue(name, out routes))
            {
                _rpcLookup[name] = routes = new Dictionary<long, Action<List<object>>>();
            }
            routes[eid] = callback;

            Action<NetNode> wrapper = new Action<NetNode>((n) =>
            {
                Call(name, entity, n);
            });

            return wrapper;
        }

        public static Action<T1, NetNode> Register<T1>(string name, Entity entity, Action<T1> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut((T1)lst[0]);
            });

            long eid = 0;
            if (entity != null)
                eid = entity.UniqueId;

            Dictionary<long, Action<List<object>>> routes;
            if (!_rpcLookup.TryGetValue(name, out routes))
            {
                _rpcLookup[name] = routes = new Dictionary<long, Action<List<object>>>();
            }
            routes[eid] = callback;

            Action<T1, NetNode> wrapper = new Action<T1,NetNode>( (p1, n) =>
            {
                Call(name, entity, p1, n);
            });

            return wrapper;
        }

        public static Action<T1, T2, NetNode> Register<T1,T2>(string name, Entity entity, Action<T1,T2> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut(
                    (T1)lst[0],
                    (T2)lst[1]
                    );
            });

            long eid = 0;
            if (entity != null)
                eid = entity.UniqueId;

            Dictionary<long, Action<List<object>>> routes;
            if (!_rpcLookup.TryGetValue(name, out routes))
            {
                _rpcLookup[name] = routes = new Dictionary<long, Action<List<object>>>();
            }
            routes[eid] = callback;

            Action<T1, T2, NetNode> wrapper = new Action<T1, T2, NetNode>((p1, p2, n) =>
            {
                Call(name, entity, p1, p2, n);
            });

            return wrapper;
        }

        public static Action<T1, T2,T3, NetNode> Register<T1,T2,T3>(string name, Entity entity, Action<T1,T2,T3> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut(
                    (T1)lst[0],
                    (T2)lst[1],
                    (T3)lst[2]
                    );
            });

            long eid = 0;
            if (entity != null)
                eid = entity.UniqueId;

            Dictionary<long, Action<List<object>>> routes;
            if (!_rpcLookup.TryGetValue(name, out routes))
            {
                _rpcLookup[name] = routes = new Dictionary<long, Action<List<object>>>();
            }
            routes[eid] = callback;

            Action<T1, T2, T3, NetNode> wrapper = new Action<T1, T2, T3, NetNode>((p1, p2, p3, n) =>
            {
                Call(name, entity, p1, p2, p3, n);
            });

            return wrapper;
        }

        public static Action<T1, T2, T3,T4, NetNode> Register<T1, T2, T3, T4>(string name, Entity entity, Action<T1, T2, T3, T4> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut(
                    (T1)lst[0],
                    (T2)lst[1],
                    (T3)lst[2],
                    (T4)lst[3]
                    );
            });

            long eid = 0;
            if (entity != null)
                eid = entity.UniqueId;

            Dictionary<long, Action<List<object>>> routes;
            if (!_rpcLookup.TryGetValue(name, out routes))
            {
                _rpcLookup[name] = routes = new Dictionary<long, Action<List<object>>>();
            }
            routes[eid] = callback;

            Action<T1, T2, T3, T4, NetNode> wrapper = new Action<T1, T2, T3, T4, NetNode>((p1, p2, p3, p4, n) =>
            {
                Call(name, entity, p1, p2, p3, p4, n);
            });

            return wrapper;
        }
        #endregion

    }
}
