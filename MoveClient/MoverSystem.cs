using Artemis;
using Asgard;
using Asgard.Client;
using Asgard.Client.Collections;
using Asgard.Core.Network;
using ChatClient;
using MoveServer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveClient
{
    public class MoverSystem : BaseSystem
    {
        public PlayerStateData StateData { get; set; }

        public List<PlayerStateData> StateList { get; set; }

        double _accum = 0;
        double _InvtickRate = 1f / 60f;
        public override bool Start()
        {
            if (StateData == null)
                StateData = new PlayerStateData();

            StateList = new List<PlayerStateData>();
            return true;
        }

        public override void Tick(double delta)
        {
            _accum += delta;
            if (_accum >= _InvtickRate)
            {
                var ticks = 0;
                double time = NetTime.SimTime;
                while (_accum >= _InvtickRate)
                {
                    _accum -= _InvtickRate;

                    UpdatePhys(_InvtickRate, time + (_InvtickRate * ticks));

                    ticks++;
                }
            }            
        }

        private void UpdatePhys(double delta, double simTime)
        {
            NetTime.SimTick++;

            var ents = this.EntityManager.GetEntities(Aspect.One(typeof(DataObject)));
            foreach(var ent in ents)
            {
                var dObject = ent.GetComponent<DataObject>();


                float perrX = (dObject.RenderX + dObject.position_error_X) - dObject.X.Value;
                float perrY = (dObject.RenderY + dObject.position_error_Y) - dObject.Y.Value;
                dObject.position_error_X = perrX;
                dObject.position_error_Y = perrY;

                dObject.RenderX = dObject.X.Value + (dObject.VelX.Value * (float)delta);
                dObject.RenderY = dObject.Y.Value + (dObject.VelY.Value * (float)delta);


                if (Math.Abs(dObject.position_error_X) >= 0.000001f)
                    dObject.position_error_X *= 0.975f;
                else
                    dObject.position_error_X = 0;

                if (Math.Abs(dObject.position_error_Y) >= 0.000001f)
                    dObject.position_error_Y *= 0.975f;
                else
                    dObject.position_error_Y = 0;
            }

//             List<MoveData> snapShot = null; //((ChatClient.MoveClient)Base).GetStateData();
//             if (snapShot != null)
//             {
//                 foreach (MoveData o in snapShot)
//                 {
//                     var activeObject = ((ChatClient.MoveClient)Base)._objects.Where(z => z.Id == o.Id).FirstOrDefault();
//                     if (activeObject == null)
//                     {
//                         activeObject = o;
//                         ((ChatClient.MoveClient)Base)._objects.Add(activeObject);
//                     }
// 
//                     float perrX = (activeObject.X + activeObject.position_error_X) - o.X;
//                     float perrY = (activeObject.Y + activeObject.position_error_Y) - o.Y;
// 
//                     activeObject.position_error_X = perrX;
//                     activeObject.position_error_Y = perrY;
// 
//                     activeObject.X = o.X;
//                     activeObject.Y = o.Y;
//                     activeObject.VelX = o.VelX;
//                     activeObject.VelY = o.VelY;
//                 }
//             }

            float x = 0f;
            float y = 0f;
            float speed = 100f;
            if (StateData.Forward)
            {
                y = -speed;
            }
            if (StateData.Back)
            {
                y = speed;
            }

            if (StateData.Right)
            {
                x = speed;
            }
            if (StateData.Left)
            {
                x = -speed;
            }

            float addX = (float)(x * delta);
            float addY = (float)(y * delta);

            StateData.X += addX;
            StateData.Y += addY;

            StateList.Add(new PlayerStateData()
            {
                X = StateData.X,
                Y = StateData.Y,
                Left = StateData.Left,
                Right = StateData.Right,
                Forward = StateData.Forward,
                Back = StateData.Back,
                Id = (int)NetTime.SimTick
            });

//             var objects = ((ChatClient.MoveClient)Base)._objects;
// //             LogHelper.Log("Tick(" + NetTime.SimTick + ") =>" +
// //                StateData.X + "," + StateData.Y + "," + addX + "," + addY, "Client");
// 
//             if (objects != null)
//             {
//                 foreach (var o in objects)
//                 {
//                     o.X += o.VelX * (float)delta;
//                     o.Y += o.VelY * (float)delta;
//                 }
//             }
// 
//             if (snapShot != null)
//             {
//                 foreach (MoveData o in ((ChatClient.MoveClient)Base)._objects)
//                 {
//                     if (Math.Abs(o.position_error_X) >= 0.000001f)
//                         o.position_error_X *= 0.975f;
//                     else
//                         o.position_error_X = 0;
// 
//                     if (Math.Abs(o.position_error_Y) >= 0.000001f)
//                         o.position_error_Y *= 0.975f;
//                     else
//                         o.position_error_Y = 0;
//                 }
//             }
        }
    }
}
