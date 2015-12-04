using Artemis.Attributes;
using Artemis.System;
using System;
using Artemis;
using System.Drawing;
using Asgard.EntitySystems.Components;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;
using System.Drawing.Imaging;

namespace MoveServer
{
    [ArtemisEntitySystem(ExecutionType = Artemis.Manager.ExecutionType.Synchronous, GameLoopType = Artemis.Manager.GameLoopType.Update, Layer = 10)]
    public class RenderSystem : EntityComponentProcessingSystem<Physics2dComponent>
    {
        Graphics _targetGraphics;
        Graphics _textLayer;
        Graphics _backBuffer;
        Bitmap _bitmap;
        Bitmap _textbitmap;

        public object LockObject = new object();

        public Graphics TextLayer
        {
            get
            {
                if (_textLayer == null)
                {
                    _textbitmap = new Bitmap(150, 100, PixelFormat.Format32bppArgb);
                    _textLayer = Graphics.FromImage(_textbitmap);
                }

                return _textLayer;
            }
        }
        public Graphics TargetGraphics 
        {
            get
            {
                return _targetGraphics;
            }
            set
            {
                _targetGraphics = value;
                _bitmap = new Bitmap(810, 610);
                _backBuffer = Graphics.FromImage(_bitmap);
            }
        }

        protected override void End()
        {
            if (_targetGraphics != null)
            {
                try
                {
                    lock(LockObject)
                    {
                        _backBuffer.DrawImage(_textbitmap, 640, 5);
                        _targetGraphics.DrawImage(_bitmap, 0, 0);
                    }
                }
                catch
                {

                }
            }
        }

        protected override void Begin()
        {
            _backBuffer.Clear(Color.White);
        }

        public override void Process(Entity entity, Physics2dComponent component)
        {
                 var body = component.Body;
 
                 _backBuffer.FillEllipse(Brushes.Red, (float)(body.Position.X * 10f) - 10f, (float)(body.Position.Y * 10f) - 10f, 20f, 20f);

        }
    }
}
