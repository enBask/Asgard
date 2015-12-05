using Artemis;
using Asgard;
using Asgard.Core.Physics;
using Asgard.Core.System;
using Asgard.EntitySystems.Components;
using ChatClient;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MoveClient
{
    public partial class MainForm : Form
    {
        private ChatClient.MoveClient moveClient;

        PlayerStateData _localState = new PlayerStateData();
        public MainForm()
        {
            InitializeComponent();
           
            moveClient = new ChatClient.MoveClient();
            moveClient.OnTick += MoveClient_OnTick;
            moveClient.PlayerState = new PlayerStateData();
            moveClient.PlayerState.Position = new Vector2(40f,30f);

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
            if (e.KeyCode == Keys.P)
            {
                moveClient.PumpNetwork(false);
            }
            if (e.KeyCode == Keys.O)
            {
                moveClient.PumpNetwork(true);
            }

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

                    var ents = moveClient.EntityManager.GetEntities(Aspect.One(typeof(Physics2dComponent)));
                    foreach(var entity in ents)
                    {
                        var dObject = entity.GetComponent<Physics2dComponent>();
                        var dataObj = entity.GetComponent<NetPhysicsObject>();
                        if (dataObj == null || dObject == null || dObject.Body == null) continue;

                        _backbuffer.FillEllipse(Brushes.Purple, ((dObject.Body.Position.X + dataObj.position_error_X) * 10f) - 10f,
                            ((dObject.Body.Position.Y + dataObj.position_error_Y) * 10f) - 10f, 20f, 20f);
                    }

                    var player = moveClient.EntityManager.GetEntityByUniqueId(1);
                    if (player != null)
                    {
                        var pComp = player.GetComponent<Physics2dComponent>();
                        var playerComp = player.GetComponent<PlayerComponent>();
                        if (playerComp != null && pComp != null && pComp.Body != null)
                        {
                            var body = pComp.Body;
                            _backbuffer.FillEllipse(Brushes.Green, (playerComp.RenderPosition.X * 10f) - 10f,
                                (playerComp.RenderPosition.Y * 10f) - 10f, 20f, 20f);

                        }
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

//         private void MoveClient_OnSnapshot(MoveServer.SnapshotPacket snapPacket)
//         {
//             if (snapPacket.DataPoints.Count == 0)
//                 return;
//             var playerData = snapPacket.DataPoints[0];
// 
//             Asgard.Core.Collections.LinkedListNode<MoveServer.MoveData> found_node = null;
//             foreach(var node in moveClient.PlayerBuffer)
//             {
//                 if ((int)node.Value.SnapId == playerData.RemoveSnapId)
//                 {
//                     //var prev_node = node.Previous;
//                     //go back one node, if we have it.
//                     //if (prev_node != null)
//                     {
//                         found_node = node;
//                         break;
//                     }
//                 }
//             }
// 
//             if (found_node != null)
//             {                
//                 moveClient.PlayerBuffer.TruncateTo(found_node);
//             }
// 
// //             lock (_backbuffer2)
// //             {
// //                 _backbuffer2.Clear(Color.White);
// //                 foreach (var obj in snapPacket.DataPoints)
// //                 {
// //                     //_backbuffer2.FillEllipse(Brushes.Blue, (float)(obj.X * 10f)-10f, (float)(obj.Y * 10f)-10f, 20f, 20f);
// //                 }
// //             }
//         }
    }
}
