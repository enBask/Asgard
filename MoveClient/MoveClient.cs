using Asgard;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using Asgard.Core.System;
using ChatServer;
using MoveClient;
using MoveServer;
using System;
using System.Collections.Generic;

namespace ChatClient
{
    public static class LogHelper
    {
        private static string _lastLog;
        public static void Log(string data, string cat = "")
        {
            if (data == _lastLog)
                return;

            _lastLog = data;

            System.Diagnostics.Trace.WriteLine(data, cat);
        }
    }

    public class MoveClient : Asgard.Client.AsgardClient<SnapshotPacket, MoveData, ClientStatePacket>
    {

        public delegate void OnTickCallback(double delta);
        public event OnTickCallback OnTick;

        BifrostClient _bifrost;

        public Asgard.Core.System.PlayerStateData PlayerState { get; set; }
        Asgard.Core.Collections.LinkedList<MoveData> _playerBuffer = new Asgard.Core.Collections.LinkedList<MoveData>();

        public List<MoveData> _objects = new List<MoveData>();

        private MoverSystem _renderSystem;
        public MoveClient() : base()
        {
            _bifrost = LookupSystem<BifrostClient>();

            _renderSystem = new MoverSystem();
            AddSystem(_renderSystem, 2);

            _bifrost.OnDisconnect += ChatClient_OnDisconnect;
            _bifrost.OnConnection += ChatClient_OnConnection;
            PacketFactory.AddCallback<LoginResponsePacket>(OnLoginResult);
        }

        public void PumpNetwork(bool pump)
        {
            _pumpNetwork = pump;
        }

        public Asgard.Core.Collections.LinkedList<MoveData> PlayerBuffer { get { return _playerBuffer; } }

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

        protected override ClientStatePacket GetClientState()
        {
//             var md = new MoveData(PlayerState.PosX, PlayerState.Y)
//             {
//                 Forward = PlayerState.Forward,
//                 Back = PlayerState.Back,
//                 Right = PlayerState.Right,
//                 Left = PlayerState.Left,
//                 SnapId = NetTime.SimTick
//             };
//             _playerBuffer.AddToTail(md);

            var mSystem = LookupSystem<MoverSystem>();

            ClientStatePacket packet = new ClientStatePacket();
            packet.State = new List<Asgard.Core.System.PlayerStateData>(mSystem.StateList);
            mSystem.StateList.Clear();
            packet.SnapId = (int)NetTime.SimTick;

            return packet;
        }

        protected override void Tick(double delta)
        {
            base.Tick(delta);
            if (OnTick != null)
                OnTick(delta);
        }
    }
}
