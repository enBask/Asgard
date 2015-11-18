using Asgard.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Packets
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketAttribute : Attribute
    {
        public ushort Id { get; set; }
        public NetDeliveryMethod Method { get; set; }

        public PacketAttribute(ushort id, NetDeliveryMethod method)
        {
            Id = id;
            Method = method;
        }

        public void BindToFactory(Type boundType)
        {
            PacketFactory.AddPacketType(boundType, Id, Method);
        }
    }
}
