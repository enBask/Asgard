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

        public override void Process(Entity entity, Physics2dComponent component1)
        {
            if (component1.Body == null) return;

            var playerComp = entity.GetComponent<PlayerData>();
            if (playerComp != null)
            {
                var body = component1.Body;
//                 if (Math.Abs(body.LinearVelocity.X) <= 00.1 && Math.Abs(body.LinearVelocity.Y) <= 00.1)
//                 {
//                     body.ApplyLinearImpulse(new Vector2(9000000000000000f, 9000000000000000f / 2f));
//                 }

                _backBuffer.FillEllipse(Brushes.Red, (float)(component1.Body.Position.X * 10f) - 10f, (float)(component1.Body.Position.Y * 10f) - 10f, 20f, 20f);
            }

            foreach (var shape in component1.Shapes)
            {
                if (shape is EdgeShape)
                {
                    var edge = (shape as EdgeShape);

                    _backBuffer.DrawLine(new Pen(Brushes.Black, 3),
                        new Point((int)(edge.Vertex1.X*10f), (int)(edge.Vertex1.Y * 10f)),
                        new Point((int)(edge.Vertex2.X * 10f), (int)(edge.Vertex2.Y * 10f)));

                }
            }

        }
    }
}
