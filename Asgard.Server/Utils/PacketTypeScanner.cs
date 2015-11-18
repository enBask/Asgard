using Asgard.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Utils
{
    internal static class PacketTypeScanner
    {
        public static void BuildPacketTypes()
        {
            var attributeType = typeof(PacketAttribute);

            var ignoredAssemblies =
            attributeType.Assembly.GetReferencedAssemblies();

            ScanAssembly(Assembly.GetEntryAssembly());
        }

        private static void ScanAssembly(Assembly assembly)
        {
            foreach(var type in assembly.GetTypes())
            {
                var packetAttribute = type.GetCustomAttribute<PacketAttribute>();
                if (packetAttribute != null)
                {
                    var boundType = type.GetType();
                    packetAttribute.BindToFactory(boundType);
                }
            }

            foreach(var refAssembly in assembly.GetReferencedAssemblies())
            {
                ScanAssembly(Assembly.Load(refAssembly));
            }
        }
    }
}
