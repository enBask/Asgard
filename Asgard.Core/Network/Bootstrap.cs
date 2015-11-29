using Asgard.Core.Network.Packets;
using Asgard.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Network
{
    public class Bootstrap
    {
        public static void Init()
        {
            InitPacketTypes();
            NetworkObjectBuilder.Compile();
        }

        private static void InitPacketTypes()
        {
            var action = new Action<TypeInfo>((type) =>
            {
                var packetAttribute = type.GetCustomAttribute<PacketAttribute>();
                if (packetAttribute != null)
                {
                    packetAttribute.BindToFactory(type);
                }
            });

            var check = new Func<TypeInfo, bool>((type) =>
             {
                 if (type.GetCustomAttribute<PacketAttribute>() != null)
                     return true;
                 else
                     return false;
             });

            AssemblyScanner.Execute<PacketAttribute>(action, check);
        }
    }
}
