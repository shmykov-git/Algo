using System;
using System.Threading.Tasks;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Models;
using Vector3 = Model3D.Vector3;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using AI.Model;
using Shape = Model.Shape;
using AI.Libraries;
using AI.NBrain;
using AI.Extensions;
using ViewMotion.Platforms.AI;
using AI.Images;
using ViewMotion.Platforms.AI.Func.T2N;
using System.IO;
using Color = System.Drawing.Color;
using ViewMotion.Platforms.AI.Func.T21;

namespace ViewMotion;

partial class SceneMotion
{
    public async Task<Motion> AI_Detector_Example()
    {
        var m = 0.9;
        var noise = 0.1;
        var smile = "☺"; // "◼";
        var smileBorderShift = 3;
        (int i, int j) imgSize0 = (200, 200);
        (int i, int j) trainCount = (50, 50);
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
                    // small image is a small hill, so let's stay only bits at its location
                    .ApplySumFilter(20)
                    .ApplyMaxPooling(10)
                    .ApplyTopBitFilter(90)
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
            Topology = [inputN, 16, 16, 16, outputN],
            LayerLinkFactors = [0.5, 0.95, 0.95, 0.95],
            PowerWeight0 = (-0.5, 0.5),
            ShaffleFactor = 0.01,
            Activator = NAct.SinB,
            Power = 2,
            EpochPerTrain = 1,
        };

        var trainer = new NTrainer(options.With(o => o.TrainData = trainBoxedData));
        trainer.Init();

        //var file = @$"d:\\ai\train.txt";
        //File.AppendAllText(file, "\r\n\r\n\r\n==================\r\n");

        //void FileWriteLine(string message)
        //{
        //    File.AppendAllText(file, $"{message}\r\n");
        //}

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
                testBoxedData.Select(d => new Vector3(d.expected[0], d.expected[1], 1)).ToPointsShape().Move(center).ToOx().ToPoints(0.2, Color.Blue),
                testBoxedData.Select((d, i) => new Vector3(d.expected[0], d.expected[1], 1-TestD(i))).ToPointsShape().Move(center).ToOx().ToPoints(0.2, Color.Red),
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

                        //FileWriteLine(bestError);
                        //FileWriteLine($"BestState: {state.bestModel.GetState().ToStateString()}");
                        //SaveImages(trainSaveImages, trainOutputs, Color.DarkGreen, @"d:\\ai\trainImg\train{0}.bmp");
                        //SaveImages(testSaveImages, testOutputs, Color.Red, @"d:\\ai\testImg\test{0}.bmp");
                        //Debug.WriteLine($"Images refreshed");
                    }
                    else
                        Debug.WriteLine($"Error: {state.bestError:E1} {state.CountInfo} {state.model.TopologyInfo} | {DateTime.Now}");
                }
            }
        }

        return await Animate().ToMotion(3);
    }


    public Task<Motion> AI21_Example()
    {
        var m = 0.75;

        return new AIMotionPlatform().AI_Learn_21(new AI21Options
        {
            m = m,
            trainN = 20,
            trainR = (-2, 2),
            modelR = (-2 / m, 2 / m),
            modelN = 50,

            frames = 900,
            showEach = 3,
            showTopology = false,
            showTopologyWeights = false,
            showError = false,
            showTime = false,
            flashSurface = false,

            mode = P21Mode.Learn,
            //learnFunc = SurfaceFuncs.Paraboloid.MoveZ(-4),
            //learnFunc = SurfaceFuncs.Hyperboloid,
            learnFunc = SurfaceFuncs.Wave(0, 4),
            //learnFunc = SurfaceFuncs.WaveX(0, 4),
            //learnFunc = SurfaceFuncs.Polynom4.MoveZ(-4),

        },
        new NOptions
        {
            Seed = 0,
            //Graph = N21Graphs.Mercury,
            //UpGraph = N21Graphs.TreeOnMercury,
            Topology = [2, 3, 4, 1],
            UpTopology = [2, 3, 4, 5, 6, 1],
            //UpTopology = [2, 6, 5, 4, 3, 1],
            AllowGrowing = true,
            AllowBelief = false,
            PowerWeight0 = (-0.05, 0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Activator = NAct.SinB,
            DynamicW0Factor = 0.01,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 2,
            LinkFactor = 0.7,
            CrossLinkFactor = 0,
            EpochPerTrain = 200,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        });
    }

    public Task<Motion> AI2N_Example()
    {
        var s = new[]
        {
            Shapes.Cube.Perfecto().Rotate(3,1,2),
            //Shapes.PlaneTorus(20, 50, 4).ToOx().Perfecto(0.9999),
        }.ToSingleShape();


        var b = s.GetBorders();
        var (from, to) = (Math.Min(b.min.x, b.min.y), Math.Max(b.max.x, b.max.y));


        var mode = P2NMode.Learn;
        var m = 0.75;
        var zN = 2;

        return new AIMotionPlatform().AI_Learn_2N(new AI2NOptions
        {
            m = m,
            trainN = 30,
            trainR = (from, to),
            modelN = 30,
            modelR = (from / m, to / m),

            frames = 1500,
            showEach = 5,
            showTopology = false,
            showTopologyWeights = false,
            showError = false,
            showTime = false,
            flashSurface = false,
            mode = mode,
            learnShape = s,
            zN = zN,
        },
        new NOptions
        {
            Seed = 1,
            Topology = [2, 8, 8, 8, 8, zN],
            UpTopology = [2, 4, 5, 6, 8, zN],
            AllowGrowing = false,
            PowerWeight0 = (-0.05, 0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Activator = NAct.LReLU,
            ReLUBias = 0.01,
            DynamicW0Factor = 0.01,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 2,
            LinkFactor = 0.75,
            CrossLinkFactor = 0,
            EpochPerTrain = 200,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        });
    }

}
