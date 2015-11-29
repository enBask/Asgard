using Artemis.Manager;
using Asgard.Core.System;

namespace Asgard
{
    public interface ISystem
    {
        bool Start();
        bool Stop();
        void Tick(double delta);

        EntityManager EntityManager { get;}

        AsgardBase Base { get; set; } 
    }

    public abstract class BaseSystem : ISystem
    {
        public EntityManager EntityManager
        {
            get;
            internal set;
        }

        public AsgardBase Base
        {
            get;
            set;
        }

        public virtual bool Start()
        {
            return true;
        }

        public virtual bool Stop()
        {
            return true;
        }

        public abstract void Tick(double delta);
    }
}
