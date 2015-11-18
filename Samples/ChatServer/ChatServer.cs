using Asgard;
using Asgard.Packets;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class ChatServer : AsgardServer, ISystem
    {
        BifrostServer _bifrost;

        Dictionary<NetConnection, string> _users = 
            new Dictionary<NetConnection, string>();

        public ChatServer()
        {
            AddSystem("chatServer", this, 1);

            PacketFactory.AddCallback<ChatPacket>(OnChatMessage);
            PacketFactory.AddCallback<ChatLoginPacket>(OnLogin);
        }


        public bool Start()
        {
            _bifrost = LookupSystem("bifrost") as BifrostServer;
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public void Tick(double delta)
        {

        }

        private void OnChatMessage(ChatPacket packet)
        {
            var conn = packet.Connection;
            string who;
            if (!_users.TryGetValue(conn, out who))
            {
                who = "";
            }

            var msg = packet.Message;
            var msgTime = NetTime.ToReadable(packet.ReceiveTime);

            Console.WriteLine("{0} : {1} - {2}", msgTime, who, msg);
        }
        private void OnLogin(ChatLoginPacket obj)
        {
            _users.Add(obj.Connection, obj.Username);

            LoginResponsePacket packet = new LoginResponsePacket();
            _bifrost.Send(packet, obj.Connection);
        }
    }
}
