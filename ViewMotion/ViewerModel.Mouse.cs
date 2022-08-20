using System.Diagnostics;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;

namespace ViewMotion;

partial class ViewerModel
{

    public void Wheel(int delta)
    {
        const double power = 0.9;

        settings.CameraOptions.Position *= delta > 0 ? power : 1 / power;

        RefreshCamera();
    }

    public void Rotate(double x, double y)
    {
        const double power = 2;
        var pr = settings.CameraOptions.LookDirection.MultS(settings.CameraOptions.Position);
        var center = settings.CameraOptions.Position - settings.CameraOptions.LookDirection.ToLenWithCheck(pr);

        var q = Quaternion.FromRotation(Vector3.ZAxis, new Vector3(power * x, -power * y, 1).Normalize());
        
        settings.CameraOptions.Position = center + (settings.CameraOptions.Position - center) * q;
        settings.CameraOptions.LookDirection *= q;

        RefreshCamera();
    }


    public void Move(double x, double y)
    {
        const double power = 1;
        var pr = settings.CameraOptions.LookDirection.MultS(settings.CameraOptions.Position).Abs();

        var dx = -settings.CameraOptions.LookDirection.MultV(settings.CameraOptions.UpDirection).ToLen(x);
        var dy = settings.CameraOptions.UpDirection.ToLen(y);

        var move = pr*power * (dx + dy);
        settings.CameraOptions.Position += move;

        RefreshCamera();
    }
}