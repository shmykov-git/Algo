using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using Mapster.Utils;
using Meta;
using Model;
using Model.Extensions;
using Model.Fourier;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using Model3D.Tools.Model;
using Model4D;
using View3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Model.Tools;
using System.Drawing.Text;
using System.Threading.Tasks.Sources;
using Model.Graphs;
using Model3D.Actives;
using Aspose.ThreeD.Entities;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics.Metrics;
using Aspose.ThreeD;
using Model.Bezier;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Mapster;
using MapsterMapper;
using AI.Model;
using Shape = Model.Shape;
using AI.Libraries;
using AI.NBrain;
using AI.Extensions;
using ViewMotion.Platforms.AI;
using AI.Images;
using System.Windows.Media;
using ViewMotion.Platforms.AI.Func.T2N;
using System.IO;
using Color = System.Drawing.Color;
using static Model3D.Actives.ActiveWorld;
using ViewMotion.Worlds;
using Meta.Extensions;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        return (Shapes.ArrowR(30, 1, 0.2, 0.7, 0.3).Perfecto().DebugJs() + Shapes.CoodsWithText()).ToMotion();

        var baseS = Surfaces.SphereAngle2(50, 10, Math.PI / 2 - Math.PI / 8, Math.PI / 2 + Math.PI / 8);
        var s = baseS.AddNormalVolume(0.05, Color.FromArgb(100, Color.Green), Color.FromArgb(100, Color.Red), Color.FromArgb(100, Color.Green));

        var s1 = s.DebugJs("1");
        var s2 = s.Mult(0.95).ToOy().DebugJs("2");
        var s3 = s.Mult(0.95*0.95).ToOx().DebugJs("3");

        return (100).SelectClosedInterval(Math.PI*2, f => s1 + s2.RotateOx(f) + s3.RotateOz(f)).ToMotion();

        //return HtmlWorlds.CubeMazeWorld();

        //return Shapes.Cube.Perfecto(20).AddNormalVolume(0.5, Color.FromArgb(100, Color.Red), Color.FromArgb(100, Color.Green)).ToMotion();

        //return Shapes.PerfectCube.Mult(40).AddNormalVolume(1, Color.Red, Color.Green).Perfecto().ToMotion();

        //Debug.WriteLine(Shapes.Cube.Perfecto(20).AddNormalVolume(0.5).Get_js_object_data());

        //var s = Shapes.ConvexEllipsoid(20, 2).Rotate(0.1).PutOn(0.5);
        ////Debug.WriteLine(s.Get_js_object_data());
        //var ashapes = (5).SelectRange(i => s.MoveY(1.2 * i).MoveX((i%2)*0.5)
        //    .ToActiveShape(o=> 
        //    {
        //        o.MaterialPower = 20;
        //        o.Skeleton.Power = 20;
        //    })).ToArray();

        //return ashapes.ToWorld(o =>
        //{
        //    o.Interaction.ParticleForce = 10;
        //}).ToMotion();

        //return (100).SelectInterval(0, Math.PI/2, fi => Shapes.ConvexEllipsoid(8, 3, fi).ToMeta()).ToMotion();

        //return Shapes.IcosahedronSp1.Perfecto().ToMeta().ToMotion();

        //var baseS = Shapes.Cube.Perfecto(20);
        //var s = baseS.AddNormalVolume(-0.5).ApplyColor(Color.FromArgb(50, Color.Blue));

        //s.Convexes = s.ConvexTriangles;

        //var l = s.Convexes.Length / 2;
        //var sss = (l/2).Range().Select(i => new Shape
        //{
        //    Points = s.Points,
        //    Convexes = new[] { s.Convexes[2*i], s.Convexes[2 * i+1], s.Convexes[2*i+l], s.Convexes[2 * i + l + 1] }
        //}).Skip(1).First().Perfecto();

        //return sss.ToMeta().ToMotion();

        //var ss = baseS.SplitByConvexes().Select(s => s.AddNormalVolume(-0.5));

        //var s = Shapes.IcosahedronSp1.Perfecto();



        //return s.ToMotion(2);


        //return Shapes.Ball.Perfecto().ApplyColor(Color.Blue).ToMotion();
        //return new Fr[] { (-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1) }.ToShape().ApplyColor(Color.FromArgb(100, Color.Red)).ToMotion();
        ////return WaterSystem.BigDee().Perfecto().ToMotion();

        //var ss = new[]
        //{
        //    Shapes.IcosahedronSp2.Perfecto().MoveX(-1).ApplyColorGradientY(Color.Red, Color.Red, Color.Red, Color.White, Color.White, Color.White),
        //    Shapes.IcosahedronSp2.Perfecto().MoveX(1).ApplyColorGradientX(Color.Green, Color.Green, Color.Green, Color.White, Color.White, Color.White)
        //};

        ////ss.ShowShapedHtml();

        //return ss.ToSingleShape().ToMotion();
    }

    public async Task<Motion> Scene1()
    {
        var m = 0.9;
        var noise = 0.1;
        var smile = "☺"; // "◼";
        var smileBorderShift = 3;
        (int i, int j) imgSize0 = (100, 100);
        (int i, int j) trainCount = (75, 75);
        int checkCount = 2000;
        int saveCount = 20;
        var smileSize = 25;
        var rectSize = (21, 21);
        double imgS0 = Math.Max(imgSize0.i, imgSize0.j);
        var distanceScale = imgS0 / m;
        var epoch = 10000;

        NBoxed[] GetBoxed(IEnumerable<NImageInfo> images, List<NImageInfo> showList, int fillCount) => images.Select(v =>
        {
            v.img.options.BoxM = m;

            if (fillCount-- > 0)
            {
                double[] expected = [NValues.Boxed(v.pos.i, v.img.m, m), NValues.Boxed(v.pos.j, v.img.n, m)];
                showList.Add(new NImageInfo { i = v.i, img = v.img.Clone(NImage.FromBit), pos = v.pos });
            }

            var res = new NBoxed
            {
                i = v.i,
                input = v.img
                    .ApplySumFilter(8)
                    .ApplyMaxPooling(4)
                    .boxedPixels,
                expected = [NValues.Boxed(v.pos.i, imgSize0.i - smileSize, m), NValues.Boxed(v.pos.j, imgSize0.j - smileSize, m)]
            };

            if (v.i % 100 == 99)
                Debug.WriteLine($"Generated images: {v.i + 1} | {DateTime.Now}");

            return res;
        }).ToArray();

        Debug.WriteLine($"Start creating train images: {DateTime.Now}");
        List<NImageInfo> trainSaveImages = new();
        var trainImages = NImages.GetSmileNoiseNetImages(trainCount, imgSize0, smile, smileSize, smileBorderShift, noise, vectorizer, rnd);
        var trainBoxedData = GetBoxed(trainImages, trainSaveImages, saveCount);
        var inputN = trainBoxedData[0].input.Length;
        var outputN = trainBoxedData[0].expected.Length;
        Debug.WriteLine($"End creating train images: {DateTime.Now}");

        List<NImageInfo> testSaveImages = new();
        var testImages = NImages.GetSmileNoiseImages(checkCount, imgSize0.i, imgSize0.j, smile, smileSize, smileBorderShift, noise, vectorizer, new Random(77));
        var testBoxedData = GetBoxed(testImages, testSaveImages, saveCount);

        SaveImages(trainSaveImages, null, Color.DarkGreen, @"d:\\ai\trainImg\train{0}.bmp");
        SaveImages(testSaveImages, null, Color.Red, @"d:\\ai\testImg\test{0}.bmp");
        Debug.WriteLine($"Images created");

        var options = new NOptions
        {
            Seed = 1,
            Topology = [inputN, 75, 25, outputN],
            LayerLinkFactors = [0.25, 0.95, 0.95, 0.95],
            AllowGrowing = false,
            PowerWeight0 = (-0.5, 0.5),
            ShaffleFactor = 0.5,
            SymmetryFactor = 0,
            Activator = NAct.ReLU,
            ReLUBias = 0.0,
            DynamicW0Factor = 0.01,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 2,
            LinkFactor = 0.45,
            CrossLinkFactor = 0,
            EpochPerTrain = 1,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        };

        var trainer = new NTrainer(options.With(o => o.TrainData = trainBoxedData));
        trainer.Init();

        var file = @$"d:\\ai\train.txt";
        File.AppendAllText(file, "\r\n\r\n\r\n==================\r\n");

        void FileWriteLine(string message)
        {
            File.AppendAllText(file, $"{message}\r\n");
        }

        void SaveImages(List<NImageInfo> images, double[][]? outputs, Color color, string file)
        {
            images.ForEach((info, i) =>
            {
                var img = info.img.Clone();
                img.DrawRect(info.pos, rectSize, Color.Blue, 2);

                if (outputs != null)
                {
                    var output = outputs[i];
                    var result = (NValues.Unboxed(outputs[info.i][0], info.img.m - smileSize, m), NValues.Unboxed(outputs[info.i][1], info.img.n - smileSize, m));
                    img.DrawRect(result, rectSize, color, 1);
                }

                img.SaveAsBitmap(string.Format(file, info.i));
            });
        }

        var center = new Vector3(-0.5, -0.5, -0.5);
        Shape GetShape(double[][] trainOutputs, double[][] testOutputs)
        {
            double TrainD(int i) => Math.Sqrt((trainBoxedData[i].expected[0] - trainOutputs[i][0]).Pow2() + (trainBoxedData[i].expected[1] - trainOutputs[i][1]).Pow2());
            double TestD(int i) => Math.Sqrt((testBoxedData[i].expected[0] - testOutputs[i][0]).Pow2() + (testBoxedData[i].expected[1] - testOutputs[i][1]).Pow2());

            return new[]
            {
                trainBoxedData.Select(d => new Vector3(d.expected[0], d.expected[1], 0)).ToPointsShape().Move(center).ToPoints(0.2, Color.Blue),
                trainBoxedData.Select((d, i) => new Vector3(d.expected[0], d.expected[1], TrainD(i))).ToPointsShape().Move(center).ToPoints(0.2, Color.Red),
                //trainOutputs.Select((o,i)=>new Vector3(o[0], o[1], TrainD(i))).ToPointsShape().Move(center).ToPoints(0.2, Color.Red),

                testBoxedData.Select(d => new Vector3(d.expected[0], d.expected[1], 1)).ToPointsShape().Move(center).ToOx().ToPoints(0.2, Color.Blue),
                testBoxedData.Select((d, i) => new Vector3(d.expected[0], d.expected[1], 1-TestD(i))).ToPointsShape().Move(center).ToOx().ToPoints(0.2, Color.Red),
                //testOutputs.Select((o,i)=>new Vector3(o[0], o[1], 1-TestD(i))).ToPointsShape().Move(center).ToOx().ToPoints(0.2, Color.Red),

                Shapes.Cube.ToLines(0.5, Color.Black)
            }.ToSingleShape();
        }

        async IAsyncEnumerable<Shape> Animate()
        {
            for (var k = 1; k < epoch / options.EpochPerTrain; k++)
            {
                var state = await trainer.Train();

                if (state.errorChanged)
                {
                    if (state.bestErrorChanged)
                    {
                        var (avgError, avgDistance, trainOutputs) = state.ComputeBestModelDataset(trainBoxedData, distanceScale);
                        var (avgTestError, avgTestDistance, testOutputs) = state.ComputeBestModelDataset(testBoxedData, distanceScale);

                        yield return GetShape(trainOutputs, testOutputs);

                        var bestError = $"\r\nBestError: {state.bestError:E1} ({avgError:E1} - {avgTestError:E1}) ({avgDistance:F1}-{avgTestDistance:F1}) {state.CountInfo} {state.bestModel.TopologyInfo} | {DateTime.Now}";
                        Debug.WriteLine(bestError);
                        FileWriteLine(bestError);
                        FileWriteLine($"BestState: {state.bestModel.GetState().ToStateString()}");

                        //SaveImages(trainSaveImages, trainOutputs, Color.DarkGreen, @"d:\\ai\trainImg\train{0}.bmp");
                        //SaveImages(testSaveImages, testOutputs, Color.Red, @"d:\\ai\testImg\test{0}.bmp");
                        //Debug.WriteLine($"Images refreshed");
                    }
                    else
                        Debug.WriteLine($"Error: {state.bestError:E1} {state.CountInfo} {state.model.TopologyInfo} | {DateTime.Now}");
                }

                if (state.isUpChanged)
                    Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().ToGraphString()}]");
            }

        }

        return await Animate().ToMotion(3);
    }
}