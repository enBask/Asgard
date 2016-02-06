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

            List<string> _loadedList = new List<string>(ignoredAssemblies.Select(a=>a.FullName));
            ScanAssembly<T>(Assembly.GetEntryAssembly().FullName, _loadedList, runner, checkFunc);
        }

        private static void ScanAssembly<T>(string assemblyName, List<string> loadedList, Action<TypeInfo> runner, Func<TypeInfo, bool> checkFunc)
            where T : class
        {
            if (loadedList.Contains(assemblyName))
                return;

            if (assemblyName.StartsWith("System."))
                return;

            if (assemblyName.StartsWith("Windows"))
                return;

            if (assemblyName.StartsWith("mscorlib"))
                return;


            loadedList.Add(assemblyName);
            var assembly = Assembly.Load(assemblyName);
            try
            {
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
            }
            catch
            {
                //swallow type load exceptions so the system can still start.
            }

            var lst = assembly.GetReferencedAssemblies();
            foreach (var refAssembly in assembly.GetReferencedAssemblies())
            {
                try
                {
                    ScanAssembly<T>(refAssembly.FullName, loadedList, runner, checkFunc);
                }
                catch (TypeLoadException)
                {
                    //swallow type load exceptions so the system can still start.
                }
            }
        }
    }
}
