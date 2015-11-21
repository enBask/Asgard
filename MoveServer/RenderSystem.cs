using Artemis.Attributes;
using Artemis.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis;
using System.Drawing;
using Asgard.EntitySystems.Components;

namespace MoveServer
{
    [ArtemisEntitySystem(ExecutionType = Artemis.Manager.ExecutionType.Synchronous, GameLoopType = Artemis.Manager.GameLoopType.Update, Layer = 10)]
    public class RenderSystem : EntityComponentProcessingSystem<Physics2dComponent>
    {
        Graphics _targetGraphics;
        Graphics _backBuffer;
        Bitmap _bitmap;
        public Graphics TargetGraphics 
        {
            get
            {
                return _targetGraphics;
            }
            set
            {
                _targetGraphics = value;
                _bitmap = new Bitmap(800, 600);
                _backBuffer = Graphics.FromImage(_bitmap);
            }
        }

        protected override void End()
        {
            if (_targetGraphics != null)
            {
                try
                {
                    _targetGraphics.DrawImage(_bitmap, 0, 0);
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
            var body = component1.Body;
            var vel = new Microsoft.Xna.Framework.Vector2(10f, 10f);
            body.LinearVelocity = vel;

            _backBuffer.FillEllipse(Brushes.Red, (float)component1.Body.Position.X, (float)component1.Body.Position.X, 20f, 20f);
        }
    }
}
