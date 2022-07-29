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
        var pr = settings.CameraOptions.LookDirection.MultS(settings.CameraOptions.Position).Abs();
        var center = settings.CameraOptions.Position + settings.CameraOptions.LookDirection.ToLen(pr);

        var q = Quaternion.FromEulerAngle(power * y, power * x, 0);
        settings.CameraOptions.Position = center + (settings.CameraOptions.Position - center) * q;
        settings.CameraOptions.LookDirection *= q;

        RefreshCamera();
    }


    public void Move(double x, double y)
    {
        const double power = 2;

        var dx = -settings.CameraOptions.LookDirection.MultV(settings.CameraOptions.UpDirection).ToLen(x);
        var dy = settings.CameraOptions.UpDirection.ToLen(y);

        var move = power * (dx + dy);
        settings.CameraOptions.Position += move;

        RefreshCamera();
    }
}