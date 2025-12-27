using Model.Extensions;
using Model3D;
using Model3D.Extensions;
using System.Windows.Input;

namespace ViewMotion;

partial class ViewerModel
{

    public void KeyDown(Key key)
    {
        if (key == Key.Space)
        {
            DoReplay();
            Refresh();
        }
    }

    public void ColorClick(int index)
    {
        if (IsColorPickerVisible)
        {
            persistState.SavedColorStates[index] = BackgroundColorState;
            SavePersistState(persistState);
            OnPropertyChanged(nameof(SavedColorBrushes));
        }
        else
        {
            BackgroundColorState = persistState.SavedColorStates[index];
        }
    }

    public void MouseClick()
    {
        IsColorPickerVisible = false;
        motion.Options.Interact(Models.InteractType.MouseClick);
    }

    public void MouseDblClick()
    {
        IsColorPickerVisible = false;
        motion.Options.Interact(Models.InteractType.MouseDblClick);
    }

    public void Wheel(int delta)
    {
        const double power = 0.9;

        motionSettings.CameraOptions.Position *= delta > 0 ? power : 1 / power;

        RefreshCamera();
    }

    public void Rotate(double x, double y)
    {
        var camera = motionSettings.CameraOptions;

        const double power = 10;
        var pr = camera.LookDirection.MultS(camera.Position);
        var center = camera.Position - camera.LookDirection.ToLenWithCheck(pr);

        var from = camera.LookDirection;
        var to = camera.LookDirection + power * y * camera.UpDirection - power * x * camera.LookDirection.MultV(camera.UpDirection);
        var q = Quaternion.FromRotation(from, to.Normalize());

        camera.Position = center + (camera.Position - center) * q;
        camera.LookDirection *= q;

        if (camera.RotateUpDirection)
            camera.UpDirection *= q;

        RefreshCamera();
    }

    public void Move(double x, double y)
    {
        const double power = 1;
        var pr = motionSettings.CameraOptions.LookDirection.MultS(motionSettings.CameraOptions.Position).Abs();

        var dx = -motionSettings.CameraOptions.LookDirection.MultV(motionSettings.CameraOptions.UpDirection).ToLen(x);
        var dy = motionSettings.CameraOptions.UpDirection.ToLen(y);

        var move = pr * power * (dx + dy);
        motionSettings.CameraOptions.Position += move;

        RefreshCamera();
    }
}