using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using Mapster.Utils;
using MathNet.Numerics;
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

namespace ViewMotion;

partial class SceneMotion
{
    public async Task<Motion> Scene()
    {
        var m = 0.75;
        var smileSize = 25;
        var rectSize = (21, 21);
        double Boxed(int x, int closeMax) => 0.5 * (1 - m) + x * m / closeMax;
        int Unboxed(double f, int closeMax) => (int)Math.Round((f - 0.5 * (1 - m)) * closeMax / m);

        var epoch = 10000;

        var testImageData = NImages.GetSmileNoiseImages(100, 64, 64, smileSize, -1, 0.1, vectorizer, new Random(77));
        testImageData.images.ForEach(v => v.img.DrawRect(v.pos, rectSize, Color.Blue, 2));
        var trainImageData = NImages.GetSmileNoiseImages(100, 64, 64, smileSize, -1, 0.1, vectorizer, rnd);
        trainImageData.images.ForEach(v => v.img.DrawRect(v.pos, rectSize, Color.Blue, 2));

        (int i, double[] input, double?[] expected)[] GetBoxedData(NImageData imgData) => 
            imgData.images.Select(v => (v.i, 
                v.img.Smooth(1).grayPixels.Select(c => Boxed(c, 0xFF)).ToArray(),
                new double?[] { Boxed(v.pos.i, imgData.m), Boxed(v.pos.j, imgData.n) })).ToArray();

        var trainBoxedData = GetBoxedData(trainImageData);
        var inputN = trainBoxedData[0].input.Length;
        var outputN = trainBoxedData[0].expected.Length;

        var options = new NOptions
        {
            Seed = 1,
            Topology = [inputN, 50, 50, outputN],
            //UpTopology = [2, 4, 5, 6, 8, zN],
            //Activators = [NAct.Softmax, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB],
            LayerLinkFactors = [0.15, 0.9, 0.9, 0.9],
            AllowGrowing = false,
            PowerWeight0 = (-0.05, 0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Activator = NAct.SinB,
            DynamicW0Factor = 0.01,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 2,
            LinkFactor = 0.45,
            CrossLinkFactor = 0,
            EpochPerTrain = 10,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        };

        var trainer = new NTrainer(options.With(o=>o.TrainData = trainBoxedData));        
        trainer.Init();

        var file = @$"d:\\train.txt";
        File.AppendAllText(file, "\r\n\r\n\r\n\r\n\r\n==================\r\n");

        void FileWriteLine(string message)
        {
            File.AppendAllText(file, $"{message}\r\n");
        }

        void Test(NModel model)
        {
            var testData = GetBoxedData(testImageData);
            var trData = testData.Select(t => t.input).ToArray();

            void SaveCollection(double[][] data, string file)
            {
                data.Index().ForEach((k, num) =>
                {
                    var img = testImageData[k].img.Clone();
                    model.ComputeOutputs(testData[k]);
                    var p = (Unboxed(model.output[0].f, testImageData[k].img.m), Unboxed(model.output[1].f, testImageData[k].img.n));
                    img.DrawRect(testImageData[k].p, rectSize, Color.Blue, 2);
                    img.DrawRect(p, rectSize, Color.Red);
                    img.SaveAsBitmap(string.Format(file, num));
                    //img.SaveAsBitmap($@"d:\\testImg\test{num}.bmp");
                });
            }

            SaveCollection(testData, @"d:\\testImg\test{0}.bmp");
            SaveCollection(trData, @"d:\\trainImg\train{0}.bmp");
        }

        Task? t = null;

        for (var k = 1; k < epoch / options.EpochPerTrain; k++)
        {
            var state = await trainer.Train();

            if (state.errorChanged)
            {
                if (state.bestErrorChanged)
                {
                    Debug.WriteLine($"\r\n{state.BestModelInfo} {state.CountInfo} {state.bestModel.TopologyInfo}");
                    FileWriteLine($"\r\n{state.BestModelInfo} {state.CountInfo} {state.bestModel.TopologyInfo}");
                    FileWriteLine($"BestState: {state.bestModel.GetState().ToStateString()}");

                    if (t == null || t.IsCompleted)
                    {
                        var ep = state.epoch;
                        t = Task.Run(() => Test(state.bestModel));
                        Debug.WriteLine($"Start refresh images for epoch {ep}");
                        _ = t.ContinueWith(_ => Debug.WriteLine($"End refresh images for epoch {ep}"));
                    }
                }
                else
                    Debug.WriteLine($"{state.ModelInfo} {state.CountInfo} {state.model.TopologyInfo}");
            }

            if (state.isUpChanged)
                Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().ToGraphString()}]");
        }

        return await Shapes.Cube.ToMotion();
    }    
}