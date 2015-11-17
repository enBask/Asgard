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
            Console.WriteLine("Starting server up on port : {0}", port);

            _server = new ServerConnection(port);


            PacketFactory.AddPacketType<MyLoginPacket>();
            PacketFactory.AddCallback<MyLoginPacket>(OnLogin);

            _server.Start();          

            while(true)
            {
                Thread.Sleep(100);
                _server.CheckStalePlayers();
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
