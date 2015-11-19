using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Utils
{
    internal static class PacketTypeScanner
    {
        public static void BuildPacketTypes()
        {
            var attributeType = typeof(PacketAttribute);

            var ignoredAssemblies =
            attributeType.Assembly.GetReferencedAssemblies();

            List<Assembly> _loadedList = new List<Assembly>();
            ScanAssembly(Assembly.GetEntryAssembly(), _loadedList);
        }

        private static void ScanAssembly(Assembly assembly, List<Assembly> loadedList)
        {
            if (loadedList.Contains(assembly))
                return;

            loadedList.Add(assembly);
            foreach (var type in assembly.GetTypes())
            {
                var packetAttribute = type.GetCustomAttribute<PacketAttribute>();
                if (packetAttribute != null)
                {
                    packetAttribute.BindToFactory(type);
                }
            }

            foreach (var refAssembly in assembly.GetReferencedAssemblies())
            {
                ScanAssembly(Assembly.Load(refAssembly), loadedList);
            }
        }
    }
}
