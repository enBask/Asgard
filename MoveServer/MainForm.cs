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

            _serverThread = new Thread(() =>
            {
                moveServer.Run();
            });
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }
    }
}
