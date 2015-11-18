using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Network
{
    public class NetNode : IEqualityComparer<NetNode>
    {
        NetConnection _connection;

        public NetNode(NetConnection conection)
        {
            _connection = conection;
        }

        public static implicit operator NetConnection(NetNode node)
        {
            return node._connection;
        }

        public static explicit operator NetNode(NetConnection connection)
        {
            return new NetNode(connection);
        }

        #region compare

        public static bool operator ==(NetNode a, NetNode b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(NetNode a, NetNode b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return (_connection.Equals(((NetNode)obj)._connection));
        }

        public override int GetHashCode()
        {
            return _connection.GetHashCode();
        }

        public bool Equals(NetNode x, NetNode y)
        {
            return x._connection.Equals(y._connection);
        }

        public int GetHashCode(NetNode obj)
        {
            return obj._connection.GetHashCode();
        }

        #endregion
    }
}
