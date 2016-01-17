using Artemis.System;
using Asgard.EntitySystems.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis;
using Asgard.Core.Network;
using System.Collections.Concurrent;
using Artemis.Attributes;
using Artemis.Manager;
using Asgard.Core.System;

namespace Asgard.EntitySystems
{
    internal interface IPlayerSystem
    {

    }

    public class PlayerSystem : EntityComponentProcessingSystem<PlayerComponent>, ISystem
    {
        ConcurrentDictionary<NetNode, Entity> _playerNodeLookup = 
            new ConcurrentDictionary<NetNode, Entity>();


        public ICollection<NetNode> Connections
        {
            get
            {
                return _playerNodeLookup.Keys;
            }
        }

        public EntityManager EntityManager
        {
            get
            {
                return EntityWorld.EntityManager;
            }
        }


        public AsgardBase Base { get; set; }


        public Entity Add(PlayerComponent comp, Entity owner = null)
        {
            if (owner == null)
            {
                owner = ObjectMapper.CreateEntity();
            }
            owner.AddComponent(comp);
            return owner;
        }

        public new void Remove(Entity player)
        {
            var playerComp = player.GetComponent<PlayerComponent>();
            if (playerComp != null)
            {
                Entity removeEntity;
                _playerNodeLookup.TryRemove(playerComp.NetworkNode, out removeEntity);
                player.RemoveComponent<PlayerComponent>();
                ObjectMapper.DestroyEntity(player);
            }
        }

        public Entity Get(NetNode node)
        {
            Entity retEntity;
            _playerNodeLookup.TryGetValue(node, out retEntity);
            return retEntity;
        }

        #region EntitySystem
        public override void Process(Entity entity, PlayerComponent playerComp)
        {         
               
        }

        public override void OnAdded(Entity entity)
        {
            var playerComp = entity.GetComponent<PlayerComponent>();
            if (playerComp.NetworkNode == null) return;
            _playerNodeLookup.TryAdd(playerComp.NetworkNode, entity);
        }

        public override void OnRemoved(Entity entity)
        {
        }

        #endregion

        #region ISystem
        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public void Tick(double delta)
        {
        }
        #endregion
    }
}
