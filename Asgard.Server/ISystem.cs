using Artemis.Manager;
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
        void Tick(float delta);

        EntityManager EntityManager { get;}
    }

    public abstract class BaseSystem : ISystem
    {
        public EntityManager EntityManager
        {
            get;
            internal set;
        }

        public virtual bool Start()
        {
            return true;
        }

        public virtual bool Stop()
        {
            return true;
        }

        public abstract void Tick(float delta);
    }
}
