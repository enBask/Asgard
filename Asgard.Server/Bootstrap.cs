using Asgard.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    public class Bootstrap
    {
        public static void Init()
        {
            InitPacketTypes();
        }

        private static void InitPacketTypes()
        {
            PacketFactory.AddPacketType<ConnectRequestPacket>();
            PacketFactory.AddPacketType<ConnectResponsePacket>();
            PacketFactory.AddPacketType<LoginResponsePacket>();
        }
    }
}
