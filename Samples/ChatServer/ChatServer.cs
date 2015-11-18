using Asgard;
using Asgard.Network;
using Asgard.Packets;
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

        Dictionary<NetNode, string> _users =
            new Dictionary<NetNode, string>();

        private static readonly DateTime s_dateInit = DateTime.Now;

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

        private DateTime GetTime(double time)
        {
            var dt = s_dateInit + TimeSpan.FromSeconds(time);
            return dt;
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

            var msgTime = GetTime(packet.ReceiveTime).ToString();
            Console.WriteLine("{0} : {1} - {2}", msgTime, who, msg);

            var connections = _users.Keys.ToList();
            packet.From = who;
            _bifrost.Send(packet, connections, conn);

        }
        private void OnLogin(ChatLoginPacket obj)
        {
            _users.Add(obj.Connection, obj.Username);

            LoginResponsePacket packet = new LoginResponsePacket();
            _bifrost.Send(packet, obj.Connection);
        }
    }
}
