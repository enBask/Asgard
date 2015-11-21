using Asgard;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using Asgard.EntitySystems;
using Asgard.EntitySystems.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Manager;

namespace ChatServer
{
    class ChatServer : AsgardServer, ISystem
    {
        BifrostServer _bifrost;
        PlayerSystem<PlayerObject> _playerSystem;
        private static readonly DateTime s_dateInit = DateTime.Now;

        public EntityManager EntityManager
        {
            get; set;
        }

        public ChatServer()
        {
            AddSystem(this, 1);

            _playerSystem = new PlayerSystem<PlayerObject>();
            AddEntitySystem(_playerSystem);

            PacketFactory.AddCallback<ChatPacket>(OnChatMessage);
            PacketFactory.AddCallback<ChatLoginPacket>(OnLogin);

            
        }


        public bool Start()
        {
            _bifrost = LookupSystem<BifrostServer>();
            _bifrost.OnDisconnect += _bifrost_OnDisconnect;
            return true;
        }

        private void _bifrost_OnDisconnect(NetNode connection)
        {
            var playerEntity = _playerSystem.Get(connection);
            if (playerEntity != null)
            {
                var playerData = playerEntity.GetComponent<PlayerObject>();

                var chatPacket = new ChatPacket();
                chatPacket.Message = playerData.DisplayName + " has disconnected";
                chatPacket.From = "SYSTEM";


                _playerSystem.Remove(playerEntity);

                _bifrost.Send(chatPacket, _playerSystem.Connections.ToList());

            }
        }

        public bool Stop()
        {
            return true;
        }

        public void Tick(float delta)
        {

        }

        private DateTime GetTime(double time)
        {
            var dt = s_dateInit + TimeSpan.FromSeconds(time);
            return dt;
        }

        private void OnChatMessage(ChatPacket packet)
        {
            var conn = packet.Connection;
            string who;
            
            var playerEntity = _playerSystem.Get(conn);
            if (playerEntity != null)
            {
                who = playerEntity.GetComponent<PlayerObject>().DisplayName;
            }
            else
            {
                who = String.Empty;
            }

            var msg = packet.Message;

            var msgTime = GetTime(packet.ReceiveTime).ToString();
            Console.WriteLine("{0} : {1} - {2}", msgTime, who, msg);

            var connections = _playerSystem.Connections.ToList();
            packet.From = who;
            _bifrost.Send(packet, connections, conn);

        }
        private void OnLogin(ChatLoginPacket obj)
        {
            LoginResponsePacket packet = new LoginResponsePacket();
            _bifrost.Send(packet, obj.Connection);


            _playerSystem.Add(new PlayerObject(               
                displayName: obj.Username,
                networkNode: obj.Connection
                ));
        }
    }
}
