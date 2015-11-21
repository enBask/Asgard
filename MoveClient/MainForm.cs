using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using MoveServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoveClient
{
    public partial class MainForm : Form
    {
        InterpolationBuffer<SnapshotPacket, MoveData> _interpolationBuffer = 
            new InterpolationBuffer<SnapshotPacket, MoveData>(30, 0.3);
        double startTime = 0.0;
        bool started = false;
        double currentTime = 0.0;


        private ChatClient.MoveClient moveClient;
        private NetNode _serverNode;
        public MainForm()
        {
            InitializeComponent();

            moveClient = new ChatClient.MoveClient("127.0.0.1", 8899);

            PacketFactory.AddCallback<SnapshotPacket>(OnSnapshot);

            var th = new Thread(() =>
            {
                moveClient.Run();
            });
            th.IsBackground = true;
            th.Start();


            var th2 = new Thread(() =>
            {                
                while(!started)
                {
                    Thread.Sleep(1);
                }

                var watch = Stopwatch.StartNew();
                var s_time = watch.Elapsed.TotalSeconds;
                while (true)
                {
                    System.Threading.Thread.Sleep(10);

                    var diff = watch.Elapsed.TotalSeconds - s_time;
                    currentTime = startTime + watch.Elapsed.TotalSeconds;
                    Render(diff);
                }
            });

            th2.IsBackground = true;
            th2.Start();

            _bitmap = new Bitmap(800, 600);
            _bitmap2 = new Bitmap(800, 600);
            _backbuffer = Graphics.FromImage(_bitmap);
            _backbuffer2 = Graphics.FromImage(_bitmap2);

        }
        Bitmap _bitmap, _bitmap2;
        Graphics _backbuffer, _backbuffer2;

        private void Render(double diff)
        {
            if (_serverNode == null) return;

            currentTime = _serverNode.GetRemoteTime(moveClient.NetTime);
            var objects = _interpolationBuffer.Update(currentTime);

            if (objects != null)
            {
                var g = CreateGraphics();
                lock(_backbuffer2)
                {
                    _backbuffer.Clear(Color.White);
                    _backbuffer.DrawImage(_bitmap2, 0, 0);
                    foreach (var obj in objects)
                    {
                        _backbuffer.FillEllipse(Brushes.Red, (float)obj.X, (float)obj.Y, 20f, 20f);
                    }
                    g.DrawImage(_bitmap, 0, 0);
                    g.Dispose();
                }
            }
        }

        private void OnSnapshot(SnapshotPacket snapPacket)
        {
            _serverNode = snapPacket.Connection;
            _interpolationBuffer.Add(snapPacket);
            if (!started)
            {
                started = true;
                startTime = snapPacket.ReceiveTime;
            }

            lock(_backbuffer2)
            {
                _backbuffer2.Clear(Color.White);
                foreach (var obj in snapPacket.DataPoints)
                {
                    _backbuffer2.FillEllipse(Brushes.Blue, (float)obj.X, (float)obj.Y, 20f, 20f);
                }
            }
        }
    }
}
