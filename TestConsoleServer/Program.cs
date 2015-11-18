using Asgard;
using Asgard.Packets;
using Asgard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsoleServer
{
    class Program
    {
        static ServerConnection _server;
        static void Main(string[] args)
        {
            int port = Config.GetInt("port");
            int max_connections = Config.GetInt("max_connections");
            Console.WriteLine("Starting server up on port : {0}", port);

            _server = new ServerConnection(port, max_connections);

            PacketFactory.AddCallback<MyLoginPacket>(OnLogin);

            _server.Start();          

            while(true)
            {
                Thread.Sleep(100);
            }
        }

        static void OnLogin(MyLoginPacket packet)
        {
            var responsePacket = new LoginResponsePacket();
            _server.Send(responsePacket, packet.Connection);
            var player = _server.AddPlayer(1, packet.Connection);
        }
    }
}
