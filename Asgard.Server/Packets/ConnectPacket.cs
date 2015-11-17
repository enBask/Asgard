using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Packets
{
    public class ConnectRequestPacket : Packet
    {
        public const string AsgardThumbPrint = "ASG0.1";
        
        public string ThumbPrint { get; private set; }
        public bool IsValid { get; private set; }

        public ConnectRequestPacket()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
        }

        public override void OnReceiveMessage()
        {
            ThumbPrint = Stream.GetString();
            IsValid = (ThumbPrint == AsgardThumbPrint);
        }

        public override void OnSendMessage()
        {
            Stream.SetString(AsgardThumbPrint);
        }
    }

    public class ConnectResponsePacket : Packet
    {
        public bool Status { get; set; }

        public ConnectResponsePacket()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
        }

        public override void OnReceiveMessage()
        {

        }

        public override void OnSendMessage()
        {
        }
    }
}
