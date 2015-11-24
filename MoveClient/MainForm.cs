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

        private ChatClient.MoveClient moveClient;
        public MainForm()
        {
            InitializeComponent();

            moveClient = new ChatClient.MoveClient();
            moveClient.OnSnapshot += MoveClient_OnSnapshot;

            var th = new Thread(() =>
            {
                moveClient.Run();
            });

            th.IsBackground = true;
            th.Start();


            var th2 = new Thread(() =>
            {                

                var watch = Stopwatch.StartNew();
                var s_time = watch.Elapsed.TotalSeconds;
                while (true)
                {
                    System.Threading.Thread.Sleep(10);
                    Render();
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

        private void Render()
        {

            var objects = moveClient.GetInterpolationObjects();
            if (objects != null)
            {
                var g = CreateGraphics();
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;

                lock(_backbuffer2)
                {
                    _backbuffer.Clear(Color.White);
                    _backbuffer.DrawImage(_bitmap2, 0, 0);
                    foreach (var obj in objects)
                    {
                        _backbuffer.FillEllipse(Brushes.Red, (obj.X*10f)-10f, (obj.Y*10f) - 10f, 20f, 20f);
                    }
                    g.DrawImage(_bitmap, 0, 0);
                    g.Dispose();
                }
            }
        }

        private void MoveClient_OnSnapshot(SnapshotPacket snapPacket)
        {
            lock (_backbuffer2)
            {
                _backbuffer2.Clear(Color.White);
                foreach (var obj in snapPacket.DataPoints)
                {
                    _backbuffer2.FillEllipse(Brushes.Blue, (float)(obj.X * 10f)-10f, (float)(obj.Y * 10f)-10f, 20f, 20f);
                }
            }
        }
    }
}
