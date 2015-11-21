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
    public class MoveClient : BifrostClient
    {
        bool _loggedIn = false;
        bool _connected = false;

        public MoveClient(string host, int port) : base(host, port)
        {
            OnDisconnect += ChatClient_OnDisconnect;
            OnConnection += ChatClient_OnConnection;
            PacketFactory.AddCallback<LoginResponsePacket>(OnLoginResult);
        }

        private void ChatClient_OnConnection(NetNode connection)
        {
            _connected = true;
            Console.WriteLine("Connected");
            Login();
        }

        private void ChatClient_OnDisconnect(NetNode connection)
        {
            _loggedIn = false;
            _connected = false;
            Console.WriteLine("Disconnected");

            Connect();
        }

        private void OnLoginResult(LoginResponsePacket packet)
        {
            Console.WriteLine("logged in!");
            _loggedIn = true;
        }

        private void Connect()
        {
            Start();
        }

        private void Login()
        {
            MoveLoginPacket loginPacket = new MoveLoginPacket();
            Send(loginPacket);
        }

        public void Run()
        {
            Connect();

            while(true)
            {
                System.Threading.Thread.Sleep(10);
                if (!_loggedIn)
                {
                    continue;
                }


            }
        }
    }
}
