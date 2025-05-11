using System;
using System.IO;
using System.Windows.Media;
using Model.Extensions;
using Model.Interfaces;
using Model3D.Extensions;
using ViewMotion.Annotations;
using ViewMotion.Models;
using Model3D;

namespace ViewMotion;

class MotionSettings : IDirSettings
{
    public CameraOptions CameraOptions { get; set; } = new()
    {
        Position = 0.5 * new Vector3(-3, 3, 10),
        LookDirection = -(0.5 * new Vector3(-3, 3, 10)).Normalize(),
        UpDirection = Vector3.YAxis,
        FieldOfView = 60
    };

    public LightOptions[] Lights { get; } =
    {
        new()
        {
            LightType = LightType.Directional,
            Color = Colors.White,
            Direction = new Vector3(-0.612372, -2.5, -0.612372)
        },
        new()
        {
            LightType = LightType.Directional,
            Color = Colors.White,
            Direction = new Vector3(0.612372, -2.5, -0.612372)
        },        
        //new()
        //{
        //    LightType = LightType.Directional,
        //    Color = Colors.White,
        //    DockSingle = new Vector3(0.612372, 2.5, 0.612372)
        //},
        //new()
        //{
        //    LightType = LightType.Directional,
        //    Color = Colors.White,
        //    DockSingle = new Vector3(-0.612372, 2.5, 0.612372)
        //},
        new()
        {
            LightType = LightType.Ambient,
            Color = Color.FromRgb(255, 255, 255)
        }
    };

    public string OutputDirectory => throw new ArgumentException(nameof(OutputDirectory));
    public string InputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\View3D\Content");
    public string GetContentFileName(string fileName) => Path.Combine(InputDirectory, fileName);
}