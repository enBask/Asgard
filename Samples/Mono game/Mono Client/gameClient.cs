using Artemis;
using Asgard.Client;
using Asgard.EntitySystems.Components;
using Microsoft.Xna.Framework.Content;
using Mono_Server;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono_Client
{
    public class gameClient : AsgardClient<ClientStatePacket>
    {
        public ContentManager Content;

    }
}
