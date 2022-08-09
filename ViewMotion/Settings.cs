using System;
using System.IO;
using System.Windows.Media;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model.Interfaces;
using Model3D.Extensions;
using ViewMotion.Models;

namespace ViewMotion;

class Settings : IDirSettings
{
    public CameraOptions CameraOptions { get; } = new()
    {
        Position = new Vector3(-3, 3, 30),
        LookDirection = -new Vector3(-3, 3, 30).Normalize(),
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
        new()
        {
            LightType = LightType.Directional,
            Color = Colors.White,
            Direction = new Vector3(0.612372, 2.5, 0.612372)
        },
        new()
        {
            LightType = LightType.Directional,
            Color = Colors.White,
            Direction = new Vector3(-0.612372, 2.5, 0.612372)
        },
        //new()
        //{
        //    LightType = LightType.Ambient,
        //    Color = Color.FromRgb(51, 51, 51)
        //}
    };

    public string OutputDirectory => throw new ArgumentException(nameof(OutputDirectory));
    public string InputDirectory => Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\View3D\Content");
    public string GetContentFileName(string fileName) => Path.Combine(InputDirectory, fileName);
}