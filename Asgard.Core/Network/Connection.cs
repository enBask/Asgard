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

namespace Asgard.Core.Network
{
    public class Connection
    {
        static Connection()
        {
        }

        public Connection()
        {

        }

        public virtual NetPeer Peer {get;}


        public double NetTime
        {
            get
            {
                return Lidgren.Network.NetTime.Now;
            }
        }

        public bool Send(Packet packet, NetNode sendTo, int channel=0)
        {
            var msg = packet.SendMessage(this);
            NetSendResult result = Peer.SendMessage(msg, (NetConnection)sendTo, (Lidgren.Network.NetDeliveryMethod)packet.Method, channel);

            if (result == NetSendResult.Queued || result == NetSendResult.Sent)
                return true;
            else
                return false;
        }
    }
}
