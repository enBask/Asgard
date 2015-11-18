using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    public interface ISystem
    {
        bool Start();
        bool Stop();
        void Tick(double delta);
    }
}
