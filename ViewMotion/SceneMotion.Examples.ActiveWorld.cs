﻿using System.Collections.Generic;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using ViewMotion.Extensions;
using ViewMotion.Models;
using static Model3D.Systems.WaterSystemPlatform;
using Model3D.Tools;
using MathNet.Numerics;
using View3D.Libraries;
using Model3D;
using Model3D.Actives;
using Model.Fourier;

namespace ViewMotion;

/// <summary>
/// You can find some video results on this youtube channel https://www.youtube.com/channel/UCSXMjRAXWmRL4rV7wy06eOA
/// </summary>
partial class SceneMotion //ActiveWorld
{
    public Task<Motion> ChristmasTreeMotion()
    {
        return (new[]
        {
            Shapes.ChristmasTree(height:1.5).ToOy().PutOn().ApplyColor(Color.Blue).ToActiveShape(o =>
            {
                o.MaterialPower = 1;
            }),
            //Parquets.Triangles(8, 13).ToShape3().Perfecto(8).ToOyM().PutOn(3).ToActiveShape(o =>
            Surfaces.Plane(15, 15).Perfecto(7).ToOyM().PutOn(3).ToActiveShape(o =>
            {
                o.ShowMeta = true;
                o.UseSkeleton = false;
                o.Color1 = Color.Yellow;
                o.Color2 = Color.Red;
                o.MetaLineMult = 1;
                o.MetaPointMult = 1;
                o.Mass = 1;
                o.MaterialPower = 15;
            }),
        }, new Shape[]
        {
            new Fr[] { (1, 1), (2, -2), (-11, 1), (-6, 2), (-9, 1), (4, 3), (-1, 12) }.ToShape().Perfecto(0.7).PutOn().Move(-0.5, 0, 0.5).ApplyColor(Color.Red)
        }).ToWorld(o =>
        {
            o.Interaction.ElasticForce = 15;
            o.Interaction.ClingForce = 0;
            o.Interaction.FrictionForce = 0;
            //o.Ground.WindPower = 0.2;
            o.Ground.UseWaves = true;
            o.Ground.WavesSize = 2;
            o.Ground.ShowGround = true;
            //o.Ground.ClingForce = 0;
            //o.Ground.FrictionForce = 0.1;
            //o.Ground.LineMult = 1;
            o.Ground.Color = Color.Blue;
        }).ToMotion(8);
    }

    public Task<Motion> TwoBallFallingToNetMotion()
    {
        var ball = Shapes.IcosahedronSp1.Perfecto();

        return new[] {
            ball.PutOn(3).Move(0.5,0,-0.5).ToActiveShape(o =>
            {
                //o.ShowMeta = true;
                o.Mass = 3;
                o.AllowTriangulation0  =false;
            }),
            ball.PutOn(2).Move(-0.2,0,0.5).ToActiveShape(o =>
            {
                o.Mass = 3;
                //o.ShowMeta = true;
                o.AllowTriangulation0  =false;
            }),
            Surfaces.Plane(20,20).Perfecto(5).ToOy()/*.RotateOz(Math.PI/6)*/.PutOn().ToActiveShape(o =>
            {
                o.Type = ActiveShapeOptions.ShapeType.D2;
                o.ShowMeta = true;
                o.AllowTriangulation0  =false;
                o.UseSkeleton = false;
                o.Mass = 1;
                o.MaterialPower = 5;
                o.Fix = new ActiveShapeOptions.FixOptions
                {
                    Dock = ActiveShapeOptions.FixDock.Left | ActiveShapeOptions.FixDock.Right
                };
            }),
        }.ToWorld(o =>
        {
            o.Ground.ShowGround = false;
            o.Ground.Y = -10;
            o.Ground.ClingForce = 0.01;
            o.InteractionType = InteractionType.ParticleWithPlane;
            o.Interaction.ElasticForce = 1;
            o.Interaction.ClingForce = 2;
            o.Interaction.FrictionForce = 2;
        }).ToMotion(10);
    }

    public Task<Motion> BallToPyramidWorldMotion()
    {
        var colors = new[] { Color.Red, Color.Blue, Color.Green, Color.Yellow };
        var ballColor = Color.SaddleBrown;

        var n = 5;
        var shiftX = 1.2;
        var shiftY = 1.028;

        //var fn = Funcs.Line();
        var fn = Funcs.Parabola(2.5 / n);
        //var fn = Funcs.Circle(0.5 * n);

        var cubes = Ranges.Pyramid2(n).Select(v =>
            Shapes.Cube/*.SplitPlanes(1)*/.PutOn() // for funcs
            .RotateOy(Math.Atan2(v.x, fn(v.x)))
            .Move(shiftX * v.x, shiftY * v.y, -2 + fn(v.x))
            .ApplyColorGradient(ExVector3.XyzAxis, Color.White, colors[(7 * v.i + 13 * v.j) % colors.Length])
            .ToActiveShape(o =>
            {
                o.Skeleton.Power = 5;
                o.MaterialPower = 5;
                o.ShowMeta = true;
            })).ToArray();

        var ball = Shapes.IcosahedronSp2.Perfecto(2).PutOn().MoveZ(4).ApplyColor(ballColor)
            .ToActiveShape(o =>
            {
                o.Skeleton.Power = 3;
                o.MaterialPower = 3;
                o.Mass = 5;
                //o.Speed = new Vector3(0, 0, -0.005);
            });

        var actives = cubes.Concat(new[] { ball });

        return actives.ToWorld(o =>
        {
            o.Interaction.ClingForce = 0;// 0.1;
            o.Interaction.FrictionForce = 0;// 0.1;
            o.Interaction.ElasticForce = 5;
            o.InteractionType = InteractionType.EdgeWithPlane;
        }).ToMotion(9);
    }

    public Task<Motion> TwoCubesWorldMotion()
    {
        var edge = 0.5;
        var cube = Shapes.Cube.SplitPlanes(edge);
        var mass = cube.PointsCount;
        var power = 2 / edge;
        var force = mass;

        var actives = new[]
        {
            cube.PutOn().MoveY(1.5).ApplyColorGradient(ExVector3.XyzAxis, Color.White, Color.Blue).ToActiveShape(o =>
            {
                o.Skeleton.Power = power;
                o.MaterialPower = power;
                o.RotationSpeedAngle = 0.003;
            }),
            cube.Scale(3,1,3).PutOn().ApplyColorGradient(ExVector3.XyzAxis, Color.White, Color.Red).ToActiveShape(o =>
            {
                o.Skeleton.Power = power;
                o.MaterialPower = power;
            }),
        };

        return actives.ToWorld(o =>
        {
            //o.InteractionType = InteractionType.EdgeWithPlane;
            o.Interaction.ElasticForce = force / 5;
            o.Interaction.FrictionForce = force / 2;
            o.Interaction.ClingForce = force;
        }).ToMotion(9);
    }

    public Task<Motion> CookieWorldMotion() => (20).SelectCirclePoints((y, x, z) => Shapes.IcosahedronSp2.Perfecto().ScaleY(0.5).PutOn().Move(0.3 * x, y * 0.6, 0.3 * z)).ToWorldMotion();

    public Task<Motion> WorldInteractionMotion()
    {
        var actives = new ActiveShape[]
            {
                Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.3).AlignY(0).MoveY(1).MoveX(-2).ApplyColorGradientX(Color.White, Color.LightBlue, Color.LightBlue, Color.LightBlue, Color.White)
                .ToActiveShape(o =>
                {
                    o.RotationSpeedAxis = Vector3.YAxis;
                    o.RotationSpeedAngle = 0.0005;
                    o.UseSkeleton = true;
                    o.Skeleton.Type = ActiveShapeOptions.SkeletonType.CenterPoint;
                    o.Skeleton.Power = 0.15;
                    o.UseBlow = true;
                    o.BlowPower = 2;
                }),

                Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.3).AlignY(0).MoveY(1).MoveX(2).ApplyColorGradientX(Color.White, Color.LightGreen, Color.LightGreen, Color.LightGreen, Color.White)
                .ToActiveShape(o =>
                {
                    o.RotationSpeedAxis = Vector3.YAxis;
                    o.RotationSpeedAngle = 0.0005;
                    o.UseSkeleton = true;
                    o.Skeleton.Type = ActiveShapeOptions.SkeletonType.CenterPoint;
                    o.Skeleton.Power = 0.15;
                    o.UseBlow = true;
                    o.BlowPower = 2;
                    o.Speed = new Vector3(-0.003, 0, 0);
                }),
            };

        // list of static shapes
        var statics = new Shape[]
            {
                vectorizer.GetText("Interaction", 200).Perfecto(7).AlignY(0).MoveZ(-4).ApplyColor(Color.SaddleBrown)
            };

        return (actives, statics).ToWorld(o =>
        {
            o.InteractionType = InteractionType.ParticleWithPlane; // Point?
            o.Interaction.ElasticForce = 1;
            o.PressurePowerMult = 0.0001;
            o.Ground.ClingForce = 0.1;
            o.Ground.FrictionForce = 0.03;
            o.MaterialThickness = 1;
            o.JediMaterialThickness = 0.5;
            o.Ground.WindPower = 2; // try wind carefully
            o.Ground.UseWaves = true;
            o.Ground.WavesSize = 2;
        }).ToMotion(10);

    }

    public Task<Motion> WorldMotion()
    {
        // list of active shapes
        var actives = new ActiveShape[]
            {
                Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.3).AlignY(0).MoveY(1).ApplyColor(Color.LightBlue)
                .ToActiveShape(o =>
                {
                    o.RotationSpeedAxis = Vector3.YAxis;
                    o.RotationSpeedAngle = 0.0005;
                    o.UseSkeleton = true;
                    o.Skeleton.Power = 0.02;
                    o.UseBlow = true;
                    o.BlowPower = 2;

                    o.OnStep += a =>
                    {                        
                        // Add any modification to animate
                        // o.BlowPower += 0.001;
                    };

                    // Add any predefined animation
                    o.OnStep += ActiveShapeAnimations.BlowUp(0.001);

                    o.OnShow += s =>
                    {
                        // change any shape options on show

                        return s; // ball.ApplyColor(Color.LightBlue);
                    };
                })
            };

        // list of static shapes
        var statics = new Shape[]
            {
                vectorizer.GetText("Wind", 200).Perfecto(7).AlignY(0).MoveZ(-4).ApplyColor(Color.SaddleBrown)
            };

        return (actives, statics).ToWorld(o =>
        {
            o.PressurePowerMult = 0.0001;
            o.Ground.ClingForce = 0.1;
            o.Ground.FrictionForce = 0.03;
            o.Ground.WindPower = 2; // try wind carefully
            o.Ground.UseWaves = true;
            o.Ground.WavesSize = 2;

            o.OnStep += w =>
            {
                // Add any world animation
            };
        }).ToMotion(10);

    }

}