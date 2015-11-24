using Asgard;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using ChatServer;
using MoveServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class MoveClient : Asgard.Client.AsgardClient<SnapshotPacket, MoveData>
    {
        BifrostClient _bifrost;

        public MoveClient() : base()
        {
            _bifrost = LookupSystem<BifrostClient>();

            _bifrost.OnDisconnect += ChatClient_OnDisconnect;
            _bifrost.OnConnection += ChatClient_OnConnection;
            PacketFactory.AddCallback<LoginResponsePacket>(OnLoginResult);
        }

        private void ChatClient_OnConnection(NetNode connection)
        {
            Console.WriteLine("Connected");
            Login();
        }

        private void ChatClient_OnDisconnect(NetNode connection)
        {
            Connect();
        }

        private void OnLoginResult(LoginResponsePacket packet)
        {
        }

        private void Connect()
        {
            _bifrost.Start();
        }

        private void Login()
        {
            MoveLoginPacket loginPacket = new MoveLoginPacket();
            _bifrost.Send(loginPacket);
        }
    }
}
