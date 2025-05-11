using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using ViewMotion.Models;
using Model3D;

namespace ViewMotion.Libraries
{
    internal static class CameraAnimations
    {
        public static CameraMotionOptions FlyAround(Vector3 cameraStartPosition, Vector3 lookDirection, double radiusMult = 0)
        {
            var radius = -lookDirection.MultS(cameraStartPosition);
            var center = cameraStartPosition + lookDirection.ToLen(radius);
            var cameraTrajectory = Trajectories.CircleTrajectory(cameraStartPosition - center, lookDirection.ToLenWithCheck(radiusMult * radius), Vector3.YAxis);
            var cameraAcceleration = Accelerations.PolyA2(0.25);

            (Vector3 pos, Vector3 look, Vector3 up) GetCamera(int step, int maxStep)
            {
                var pos = cameraTrajectory(cameraAcceleration((double)step / maxStep)); // step / (n - 1.0)
                var look = -pos.Normalize();
                var up = Vector3.YAxis;

                return (pos + center, look, up);
            }

            return new CameraMotionOptions()
            {
                CameraStartOptions = new CameraOptions()
                {
                    Position = cameraStartPosition,
                    LookDirection = lookDirection,
                    UpDirection = Vector3.YAxis,
                    FieldOfView = 60,
                    RotateUpDirection = false,
                },
                CameraFn = GetCamera
            };
        }
    }
}
