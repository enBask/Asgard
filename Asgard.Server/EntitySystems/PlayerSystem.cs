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

namespace Asgard.EntitySystems
{
    internal interface IPlayerSystem
    {

    }

    public class PlayerSystem<PlayerType> : EntityComponentProcessingSystem<PlayerType>, ISystem
        where PlayerType : PlayerComponent
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

        public Entity Add(PlayerType comp, Entity owner = null)
        {
            if (owner == null)
            {
                owner = EntityWorld.CreateEntity();
            }
            owner.AddComponent(comp);
            return owner;
        }

        public new void Remove(Entity player)
        {
            var playerComp = player.GetComponent<PlayerType>();
            if (playerComp != null)
            {
                Entity removeEntity;
                _playerNodeLookup.TryRemove(playerComp.NetworkNode, out removeEntity);
                player.RemoveComponent<PlayerType>();
            }
        }

        public Entity Get(NetNode node)
        {
            Entity retEntity;
            _playerNodeLookup.TryGetValue(node, out retEntity);
            return retEntity;
        }

        #region EntitySystem
        public override void Process(Entity entity, PlayerType playerComp)
        {         
               
        }

        public override void OnAdded(Entity entity)
        {
            var playerComp = entity.GetComponent<PlayerType>();
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

        public void Tick(float delta)
        {
        }
        #endregion
    }
}
