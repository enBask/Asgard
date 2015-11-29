using Asgard.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Interpolation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InterpolationAttribute : Attribute
    {
    }

    public interface IInterpolationPacket<TData>
    {
        uint Id { get; set; }

        List<TData> DataPoints { get; set; }
    }
}
