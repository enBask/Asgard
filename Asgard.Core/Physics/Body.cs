using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Physics
{
    public class Body
    {
        internal Vector2 _position;
        internal Vector2 _linearVelocity;
        internal object _userData;
        internal bool _sleeping;

        public Body(BodyDefinition definition)
        {
            _position = definition.Position;
            _linearVelocity = definition.LinearVelocity;
            _sleeping = false;
        }

        public object UserData
        {
            get
            {
                return _userData;
            }
            set
            {
                _userData = value;
            }
        }

        public bool Awake
        {
            get
            {
                return !_sleeping;
            }
            set
            {
                _sleeping = !value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                _sleeping = false;
            }
        }

        public Vector2 LinearVelocity
        {
            get
            {
                return _linearVelocity;
            }
            set
            {
                _linearVelocity = value;
                _sleeping = false;
            }
        }
    }
}
