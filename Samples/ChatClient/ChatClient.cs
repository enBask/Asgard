using Asgard;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using ChatServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class ChatClient : BifrostClient
    {
        bool _loggedIn = false;
        bool _connected = false;

        public ChatClient(string host, int port) : base(host, port)
        {
            OnDisconnect += ChatClient_OnDisconnect;
            OnConnection += ChatClient_OnConnection;
            PacketFactory.AddCallback<LoginResponsePacket>(OnLoginResult);
            PacketFactory.AddCallback<ChatPacket>(OnChatMessage);
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

            //Connect();
        }

        private void OnChatMessage(ChatPacket packet)
        {
            var who = packet.From;  
            var msg = packet.Message;

            Console.WriteLine("{0} - {1}", who, msg);
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
            Console.Write("Username: ");
            var name = Console.ReadLine();
            ChatLoginPacket loginPacket = new ChatLoginPacket();
            loginPacket.Username = name;
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

                string line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    ChatPacket packet = new ChatPacket();
                    packet.Message = line;
                    Send(packet);
                }
            }
        }
    }
}
