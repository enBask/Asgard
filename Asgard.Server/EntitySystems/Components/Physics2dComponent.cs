using Artemis.Interface;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.EntitySystems.Components
{
    public class Physics2dComponent :  IComponent
    {
        public int WorldID { get; set; }

        public Body Body { get; internal set; }

        public Vector2 StartingPosition { get; set; }

        public BodyType BodyType { get; set; }
       
    }
}
