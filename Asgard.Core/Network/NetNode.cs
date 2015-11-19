using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network
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
            if (Object.ReferenceEquals(a, b)) return true;

            if ((object)a == null || (object)b == null) return false;

            return a.Equals(b);
        }


        public static bool operator !=(NetNode a, NetNode b)
        {
            if (Object.ReferenceEquals(a, b)) return false;

            if ((object)a == null || (object)b == null) return true;

            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return (_connection.Equals(((NetNode)obj)._connection));
        }

        public override int GetHashCode()
        {
            if (_connection == null) return 0;
            return _connection.GetHashCode();
        }

        public bool Equals(NetNode x, NetNode y)
        {
            if (Object.ReferenceEquals(x,y)) return true;
            if ((object)x == null || (object)y == null) return false;

            return x._connection.Equals(y._connection);
        }

        public int GetHashCode(NetNode obj)
        {
            return obj._connection.GetHashCode();
        }

        #endregion
    }
}
