using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model.Libraries;
using Model3D.Libraries;
using ViewMotion.Models;

namespace ViewMotion.Libraries
{
    internal static class CameraAnimations
    {
        public static CameraMotionOptions FlyAround(Vector3 cameraStartPosition)
        {
            var cameraTrajectory = Trajectories.CircleTrajectory(cameraStartPosition, Vector3.Origin, Vector3.YAxis);
            var cameraAcceleration = Accelerations.Poly2;

            (Vector3 pos, Vector3 look, Vector3 up) GetCamera(int step, int maxStep)
            {
                var pos = cameraTrajectory(cameraAcceleration((double)step / maxStep)); // step / (n - 1.0)
                var look = -pos.Normalize();
                var up = Vector3.YAxis;

                return (pos, look, up);
            }

            return new CameraMotionOptions()
            {
                CameraStartOptions = new CameraOptions()
                {
                    Position = cameraStartPosition,
                    LookDirection = -cameraStartPosition.Normalize(),
                    UpDirection = Vector3.YAxis,
                    FieldOfView = 60,
                    RotateUpDirection = false,
                },
                CameraFn = GetCamera
            };
        }
    }
}
