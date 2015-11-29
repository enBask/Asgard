using Artemis;
using Asgard;
using Asgard.Client.Collections;
using Asgard.Core.Network;
using Asgard.Core.Network.Packets;
using Asgard.Core.System;
using ChatClient;
using MoveServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

        int tickRate = 60;
        float _downTime = 0f;
        PlayerStateData _localState = new PlayerStateData();
        public MainForm()
        {
            InitializeComponent();
           
            moveClient = new ChatClient.MoveClient();
            moveClient.OnTick += MoveClient_OnTick;
            moveClient.OnSnapshot += MoveClient_OnSnapshot;
            moveClient.PlayerState = new PlayerStateData();
            moveClient.PlayerState.X = 40;
            moveClient.PlayerState.Y = 30;

            var moveSys = moveClient.LookupSystem<MoverSystem>();
            moveSys.StateData = moveClient.PlayerState;

            var th = new Thread(() =>
            {
                moveClient.Run();
            });

            th.IsBackground = true;
            th.Start();

            var th3 = new Thread(() =>
            {
                while (true)
                {
                    var fontStyle = FontStyle.Bold;
                    //Thread.Sleep(1);
                    var net = moveClient.LookupSystem<BifrostClient>();
                    if (net != null)
                    {
                        var stats = net.GetStats();
                        Thread.Sleep(1000);
                        var stats2 = net.GetStats();

                        if (stats != null && stats2 != null)
                        {
                            var oDiff = stats2.BytesOutPerSec - stats.BytesOutPerSec;
                            var iDiff = stats2.BytesInPerSec - stats.BytesInPerSec;
                            //oDiff = (float)oDiff;
                            var outBytes = Math.Round(oDiff / 1024f * 8f, 2);
                            var inBytes = Math.Round(iDiff / 1024f * 8f, 2);
                            try
                            {
                                lock (_backbuffer2)
                                {
                                    var g = _textGraphics;
                                    if (g != null)
                                    {
                                        g.Clear(Color.White);
                                        g.DrawString("Out kbps: " + outBytes,
                                            new Font(FontFamily.GenericMonospace, 12, fontStyle), Brushes.Black, 0, 0);
                                        g.DrawString("In kbps : " + inBytes,
                                            new Font(FontFamily.GenericMonospace, 12, fontStyle), Brushes.Black, 0, 15);
                                        g.DrawString("Ping : " + Math.Round(stats2.AvgPing * 1000.0,2) + "ms",
                                            new Font(FontFamily.GenericMonospace, 12, fontStyle), Brushes.Black, 0, 30);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            });

            th3.IsBackground = true;
            th3.Start();

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

            _textbitmap = new Bitmap(150, 100, PixelFormat.Format32bppArgb);
            _textGraphics = Graphics.FromImage(_textbitmap);
        }



        Bitmap _bitmap, _bitmap2;
        Graphics _backbuffer, _backbuffer2;

        Bitmap _textbitmap;
        Graphics _textGraphics;


        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            bool bChanged = false;
            if (e.KeyCode == Keys.Up)
            {
                _localState.Forward = true;
                bChanged = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                _localState.Back = true;
                bChanged = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                _localState.Left = true;
                bChanged = true;
            }
            if (e.KeyCode == Keys.Right)
            {
                _localState.Right = true;
                bChanged = true;
            }           
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            bool bChanged = false;
            if (e.KeyCode == Keys.Up)
            {
                _localState.Forward = false;
                bChanged = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                _localState.Back = false;
                bChanged = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                _localState.Left = false;
                bChanged = true;
            }
            if (e.KeyCode == Keys.Right)
            {
                _localState.Right = false;
                bChanged = true;
            }
        }

        Dictionary<int, MoveData> _objects = new Dictionary<int, MoveData>();
        private void Render()
        {           
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
                    _backbuffer.DrawImage(_textbitmap, 640, 5);

                    foreach (var obj in moveClient._objects)
                    {                       
                        _backbuffer.FillEllipse(Brushes.Red, ((obj.X + obj.position_error_X) * 10f) - 10f, ((obj.Y + obj.position_error_Y) * 10f) - 10f, 20f, 20f);
                    }

                    var xDiff = Math.Abs(moveClient.PlayerState.RenderX - moveClient.PlayerState.X);
                    var yDiff = Math.Abs(moveClient.PlayerState.RenderY - moveClient.PlayerState.Y);

//                     if ( (xDiff >= 2.0f || yDiff >= 2.0f) 
//                         )
//                     {
//                         moveClient.PlayerState.RenderX = moveClient.PlayerState.X;
//                         moveClient.PlayerState.RenderY = moveClient.PlayerState.Y;
//                     }
//                     else
//                     {
//                         moveClient.PlayerState.RenderX = MathHelpers.LinearInterpolate(moveClient.PlayerState.RenderX,
//                             moveClient.PlayerState.X, 0.2f);
//                         moveClient.PlayerState.RenderY = MathHelpers.LinearInterpolate(moveClient.PlayerState.RenderY,
//                             moveClient.PlayerState.Y, 0.2f);
//                     }

                    _backbuffer.FillEllipse(Brushes.Green, (moveClient.PlayerState.X * 10f) - 10f,
                        (moveClient.PlayerState.Y * 10f) - 10f, 20f, 20f);


                    var moveSys = moveClient.LookupSystem<MoverSystem>();
                    var ents = moveSys.EntityManager.GetEntities(Aspect.One(typeof(MoveServer.DataObject)));
                    foreach(var entity in ents)
                    {
                        var dObject = entity.GetComponent<MoveServer.DataObject>();
                        _backbuffer.FillEllipse(Brushes.Purple, ((dObject.RenderX+dObject.position_error_X) * 10f) - 10f,
                            ((dObject.RenderY + dObject.position_error_Y) * 10f) - 10f, 20f, 20f);
                    }

                    g.DrawImage(_bitmap, 0, 0);
                    g.Dispose();
                }
            }
        }

        private void MoveClient_OnTick(double delta)
        {
            moveClient.PlayerState.Forward = _localState.Forward;
            moveClient.PlayerState.Back = _localState.Back;
            moveClient.PlayerState.Left = _localState.Left;
            moveClient.PlayerState.Right = _localState.Right;
        }

        private void MoveClient_OnSnapshot(SnapshotPacket snapPacket)
        {
            if (snapPacket.DataPoints.Count == 0)
                return;
            var playerData = snapPacket.DataPoints[0];

//             moveClient.PlayerState.X = playerData.X;
//             moveClient.PlayerState.Y = playerData.Y;

            //find the first node where locaLTime > node.Time

            Asgard.Core.Collections.LinkedListNode<MoveData> found_node = null;
            foreach(var node in moveClient.PlayerBuffer)
            {
                if ((int)node.Value.SnapId == playerData.RemoveSnapId)
                {
                    //var prev_node = node.Previous;
                    //go back one node, if we have it.
                    //if (prev_node != null)
                    {
                        found_node = node;
                        break;
                    }
                }
            }

            if (found_node != null)
            {                
                moveClient.PlayerBuffer.TruncateTo(found_node);
            }

//             lock (_backbuffer2)
//             {
//                 _backbuffer2.Clear(Color.White);
//                 foreach (var obj in snapPacket.DataPoints)
//                 {
//                     //_backbuffer2.FillEllipse(Brushes.Blue, (float)(obj.X * 10f)-10f, (float)(obj.Y * 10f)-10f, 20f, 20f);
//                 }
//             }
        }
    }
}
