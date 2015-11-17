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
        private double _lastPacketTime;

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

        public void UpdateStaleState(double packetTime)
        {
            _lastPacketTime = packetTime;
        }
        
        public bool IsStale(double range=30.0)
        {
            var diff = NetTime.Now - _lastPacketTime;
            return (diff >= range);
        }
 
            
    }
}
