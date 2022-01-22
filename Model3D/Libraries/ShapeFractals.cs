using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using View3D.Libraries;

namespace Model3D.Libraries
{
    public static class ShapeFractals
    {
        public static Shape NeverMindTree(int n) => new ShapeTreeFractal
        {
            Steps = new[]
            {
                new ShapeTreeFractal.Step()
                {
                    Shape = Surfaces.Cylinder(8, 41).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.ParabolaY).Rotate(Rotates.Y_Z),
                    Rules = new []
                    {
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(0, 2, 4),
                            Direction = new Vector3(0, 0, 1),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(0, -2, 4),
                            Direction = new Vector3(0, 0, 1),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        }
                    }
                }
            }
        }.CreateFractal(n);

        public static Shape NeverMindTree3D(int n) => new ShapeTreeFractal
        {
            Steps = new[]
            {
                new ShapeTreeFractal.Step()
                {
                    Shape = Surfaces.Cylinder(8, 41).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.ParabolaY).Rotate(Rotates.Y_Z) +
                            Surfaces.Cylinder(8, 41).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.ParabolaY).Rotate(Rotates.Y_Z).Rotate(Rotates.Y_X),
                    Rules = new []
                    {
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(0, 2, 4),
                            Direction = new Vector3(0, 0, 1),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(0, -2, 4),
                            Direction = new Vector3(0, 0, 1),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(2, 0, 4),
                            Direction = new Vector3(0, 0, 1),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(-2, 0, 4),
                            Direction = new Vector3(0, 0, 1),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                    }
                }
            }
        }.CreateFractal(n);

        public static Shape ParabolaTree(int n) => new ShapeTreeFractal
        {
            Steps = new[]
            {
                new ShapeTreeFractal.Step()
                {
                    Shape = Surfaces.Cylinder(8, 41).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.ParabolaY).Rotate(Rotates.Y_Z) +
                            Surfaces.Cylinder(8, 41).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.ParabolaY).Rotate(Rotates.Y_Z).Rotate(Rotates.Y_X),
                    Rules = new []
                    {
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(0, 2, 4),
                            Direction = new Vector3(0, 2, 4).Normalize(),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(0, -2, 4),
                            Direction = new Vector3(0, -2, 4).Normalize(),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(2, 0, 4),
                            Direction = new Vector3(2, 0, 4).Normalize(),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                        new ShapeTreeFractal.Rule
                        {
                            Point = new Vector3(-2, 0, 4),
                            Direction =  new Vector3(-2, 0, 4).Normalize(),
                            Scale = (5.0.Sqrt() - 1) / 2.5
                        },
                    }
                }
            }
        }.CreateFractal(n);
    }


}
