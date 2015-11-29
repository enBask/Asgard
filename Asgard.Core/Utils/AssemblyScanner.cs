using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Utils
{
    internal static class AssemblyScanner
    {
        public static void Execute<T>(Action<TypeInfo> runner, Func<TypeInfo,bool> checkFunc=null)
             where T : class
        {
            var attributeType = typeof(T);

            var ignoredAssemblies =
            attributeType.Assembly.GetReferencedAssemblies();

            List<Assembly> _loadedList = new List<Assembly>();
            ScanAssembly<T>(Assembly.GetEntryAssembly(), _loadedList, runner, checkFunc);
        }

        private static void ScanAssembly<T>(Assembly assembly, List<Assembly> loadedList, Action<TypeInfo> runner, Func<TypeInfo, bool> checkFunc)
            where T : class
        {
            if (loadedList.Contains(assembly))
                return;

            loadedList.Add(assembly);
            foreach (var type in assembly.DefinedTypes)
            {
                if (checkFunc == null)
                {
                    runner(type);
                }
                else
                {
                    if (checkFunc(type))
                    {
                        runner(type);
                    }
                }
            }

            foreach (var refAssembly in assembly.GetReferencedAssemblies())
            {
                ScanAssembly<T>(Assembly.Load(refAssembly), loadedList, runner, checkFunc);
            }
        }
    }
}
