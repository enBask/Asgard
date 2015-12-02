using Artemis;
using Asgard;
using Asgard.Core.Network;
using Asgard.Core.System;
using MoveServer;
using System;
using System.Collections.Generic;
using System.Numerics;

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

                bool applyX = true, applyY = true;
//                 if (dObject.PrevX != dObject.X)
//                 {
//                     LogHelper.Log("snapping to X");
//                     dObject.PrevX = dObject.X;
//                     dObject.RenderX = dObject.X;
// 
//                     applyX = (dObject.VelX == 0f);
//                 }
// 
//                 if (dObject.PrevY != dObject.Y)
//                 {
//                     LogHelper.Log("snapping to Y");
//                     dObject.PrevY = dObject.Y;
//                     dObject.RenderY = dObject.Y;
// 
//                     applyY = (dObject.VelY == 0f);
//                 }

                float perrX = (dObject.RenderX + dObject.position_error_X) - dObject.X;
                float perrY = (dObject.RenderY + dObject.position_error_Y) - dObject.Y;
                dObject.position_error_X = perrX;
                dObject.position_error_Y = perrY;
                


                if (applyX)
                    dObject.RenderX = dObject.X + (dObject.VelX * (float)delta);
                if (applyY)
                    dObject.RenderY = dObject.Y + (dObject.VelY * (float)delta);


                if (Math.Abs(dObject.position_error_X) >= 0.00001f)
                    if (Math.Abs(dObject.position_error_X) >= 1f)
                        dObject.position_error_X *= 0.975f;
                    else
                        dObject.position_error_X *= 0.975f;
                else
                    dObject.position_error_X = 0;

                if (Math.Abs(dObject.position_error_Y) >= 0.00001f)
                    if (Math.Abs(dObject.position_error_Y) >= 1f)
                        dObject.position_error_Y *= 0.975f;
                    else
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

            Vector2 vel = new Vector2();
            float speed = 25f;
            if (StateData.Forward)
            {
                vel.Y = -speed;
            }
            if (StateData.Back)
            {
                vel.Y = speed;
            }

            if (StateData.Right)
            {
                vel.X = speed;
            }
            if (StateData.Left)
            {
                vel.X = -speed;
            }

            vel *= (float)delta;

            StateData.Position += vel;

            StateList.Add(new PlayerStateData()
            {
                Position = StateData.Position,
                Left = StateData.Left,
                Right = StateData.Right,
                Forward = StateData.Forward,
                Back = StateData.Back
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
