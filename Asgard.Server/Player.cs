using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    public class Player
    {

        public NetConnection Connection { get; private set; }
        public ushort Id { get; private set; }

        public Player(NetConnection connection, ushort id)
        {
            Connection = connection;
            Id = id;
        }

        public void ResetConnection(NetConnection connection)
        {
            Connection = connection;
        }
            
    }
}
