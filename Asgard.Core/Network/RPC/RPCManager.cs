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
        static Dictionary<string, Action<List<object>>> _rpcLookup;

        static Connection _connection;
        static RPCManager()
        {
            _rpcLookup = new Dictionary<string, Action<List<object>>>();
        }

        public static void SetConnection(Connection conn)
        {
            _connection = conn;
        }

        internal static void Call(string name, List<object> parms)
        {
            Action<List<object>> callback = null;
            if (_rpcLookup.TryGetValue(name, out callback))
            {
                callback(parms);
            }
        }

        #region Call methods
        public static void Call<T>(string name, NetNode node, T p1)
        {
            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            _connection.Send(packet, node, 10);
        }
        public static void Call<T1, T2>(string name, NetNode node, T1 p1, T2 p2)
        {
            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            packet.Parameters.Add(p2);
            _connection.Send(packet, node, 10);
        }
        public static void Call<T1, T2, T3>(string name, NetNode node, T1 p1, T2 p2, T3 p3)
        {
            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            packet.Parameters.Add(p2);
            packet.Parameters.Add(p3);
            _connection.Send(packet, node, 10);
        }
        public static void Call<T1, T2, T3, T4>(string name, NetNode node, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            RPCPacket packet = new RPCPacket();
            packet.Name = name;
            packet.Parameters = new List<object>();
            packet.Parameters.Add(p1);
            packet.Parameters.Add(p2);
            packet.Parameters.Add(p3);
            packet.Parameters.Add(p4);
            _connection.Send(packet, node, 10);
        }
        #endregion

        #region Register Methods
        public static Action<T1, NetNode> Register<T1>(string name, Action<T1> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut((T1)lst[0]);
            });

            _rpcLookup[name] = callback;

            Action<T1, NetNode> wrapper = new Action<T1,NetNode>( (p1, n) =>
            {
                Call(name, n, p1);
            });

            return wrapper;
        }

        public static Action<T1, T2, NetNode> Register<T1,T2>(string name, Action<T1,T2> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut(
                    (T1)lst[0],
                    (T2)lst[1]
                    );
            });

            _rpcLookup[name] = callback;

            Action<T1, T2, NetNode> wrapper = new Action<T1, T2, NetNode>((p1, p2, n) =>
            {
                Call(name, n, p1, p2);
            });

            return wrapper;
        }

        public static Action<T1, T2,T3, NetNode> Register<T1,T2,T3>(string name, Action<T1,T2,T3> callOut)
        {
            Action<List<object>> callback = new Action<List<object>>(lst =>
            {
                callOut(
                    (T1)lst[0],
                    (T2)lst[1],
                    (T3)lst[2]
                    );
            });

            _rpcLookup[name] = callback;

            Action<T1, T2, T3, NetNode> wrapper = new Action<T1, T2, T3, NetNode>((p1, p2, p3, n) =>
            {
                Call(name, n, p1, p2, p3);
            });

            return wrapper;
        }

        public static Action<T1, T2, T3,T4, NetNode> Register<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> callOut)
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

            _rpcLookup[name] = callback;

            Action<T1, T2, T3, T4, NetNode> wrapper = new Action<T1, T2, T3, T4, NetNode>((p1, p2, p3, p4, n) =>
            {
                Call(name, n, p1, p2, p3, p4);
            });

            return wrapper;
        }
        #endregion

    }
}
