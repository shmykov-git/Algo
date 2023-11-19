using System;
using System.Drawing;
using Aspose.ThreeD.Utilities;
using Meta.Extensions;
using Model;

namespace Model3D.Actives;

public class ActiveShapeOptions
{
    public event Action<ActiveShape> OnStep;
    public event Func<Shape, Shape> OnShow;

    public void Step(ActiveShape activeShape) => OnStep?.Invoke(activeShape);
    public Shape Show(Shape shape) => OnShow.RollRaise(shape);

    public ActiveWorldOptions WorldOptions { get; set; }

    public int StepNumber { get; set; }
    public bool ShowMeta { get; set; }
    public double MetaLineMult { get; set; }
    public double MetaPointMult { get; set; }
    public Color? Color1 { get; set; }
    public Color? Color2 { get; set; }
    public double RotationAngle { get; set; }
    public Vector3 RotationAxis { get; set; }
    public Vector3? RotationCenter { get; set; }
    public Vector3 Speed { get; set; }
    public double Mass { get; set; }
    public double RotationSpeedAngle { get; set; }
    public Vector3 RotationSpeedAxis { get; set; }
    public Vector3? RotationSpeedCenter { get; set; }
    public bool UseSkeleton { get; set; }
    public SkeletonOptions Skeleton { get; set; }
    public double MaterialPower { get; set; }
    public double MaterialThickness { get; set; }
    public double JediMaterialThickness { get; set; }
    public bool UseMaterialDamping { get; set; }
    public double MaterialDamping { get; set; }
    public bool UseBlow {  get; set; }
    public double BlowPower { get; set; }
    public FixOptions Fix { get; set; }
    public bool UseInteractions { get; set; }
    public bool UseSelfInteractions { get; set; }
    public Vector3 ColliderScale { get; set; }
    public enum FixDock
    {
        None = 0,
        Point,
        Left,
        Top,
        Right,
        Bottom,
        Back,
        Front
    }

    public enum SkeletonType
    {
        CenterPoint,
        ShapeSizeRatioRadius,
        Radius
    }

    public class SkeletonOptions
    {
        public double Power { get; set; }
        public SkeletonType Type { get; set; }
        public double Radius { get; set; }
        public bool ShowPoints { get; set; }
    }

    public class FixOptions
    {
        public FixDock Dock { get; set; } = FixDock.None;
        public Vector3 Point { get; set; }
        public Vector3 Direction { get; set; } = Vector3.ZAxis;
        public double Distance { get; set; } = 0.1;
    }

    public void With(Action<ActiveShapeOptions> action) => action(this);
}
