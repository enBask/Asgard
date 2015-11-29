using Asgard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoveServer
{
    public partial class MainForm : Form
    {
        Thread _serverThread;

        public MainForm()
        {
            InitializeComponent();

            var moveServer = new MoveServer();

            var renderSystem = moveServer.GetEntitySystem<RenderSystem>();
            renderSystem.TargetGraphics = CreateGraphics();

            moveServer.Start();
            _serverThread = new Thread(() =>
            {
                moveServer.Run();
            });
            _serverThread.IsBackground = true;
            _serverThread.Start();

            var th = new Thread(() =>
            {
                while (true)
                {
                    var fontStyle = FontStyle.Bold;
                    //Thread.Sleep(1);
                    var net = moveServer.LookupSystem<BifrostServer>();
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
                                lock(renderSystem.LockObject)
                                {
                                    var g = renderSystem.TextLayer;
                                    if (g != null)
                                    {
                                        g.Clear(Color.White);
                                        g.DrawString("Out kbps: " + outBytes,
                                            new Font(FontFamily.GenericMonospace, 12, fontStyle), Brushes.Black, 0, 0);
                                        g.DrawString("In kbps : " + inBytes,
                                            new Font(FontFamily.GenericMonospace, 12, fontStyle), Brushes.Black, 0, 15);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            });
            th.IsBackground = true;
            th.Start();
        }
    }
}
