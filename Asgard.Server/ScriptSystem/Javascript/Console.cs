using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.ScriptSystem.Javascript
{
    internal class Console
    {
        public void log(object data)
        {
            try
            {
                Trace.WriteLine(data.ToString());
            }
            catch(Exception e)
            {

            }
        }
    }
}
