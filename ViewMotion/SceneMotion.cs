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
        (int i, int j) imgSize = (64, 64);
        int trainCount = 10000;
        int checkCount = 100;
        var smileSize = 25;
        var rectSize = (21, 21);
        var errorScale = Math.Max(imgSize.i, imgSize.j) / m;
        //double Boxed(int x, int ln) => 0.5 * (1 - m) + x * m / ln;
        //int Unboxed(double f, int ln) => (int)Math.Round((f - 0.5 * (1 - m)) * ln / m);

        var epoch = 10000;

        NBoxed[] GetBoxed(NImageData imgData) => imgData.images.Select(v => new NBoxed
        {
            i = v.i,
            input = v.img.grayPixels.Select(c => NValues.Boxed(c, 255, m)).ToArray(),
            expected = [NValues.Boxed(v.pos.i, imgData.m, m), NValues.Boxed(v.pos.j, imgData.n, m)]
        }).ToArray();

        var trainImageData = NImages.GetSmileNoiseImages(trainCount, imgSize.i, imgSize.j, smileSize, -1, 0.1, vectorizer, rnd);
        var trainBoxedData = GetBoxed(trainImageData);
        var inputN = trainBoxedData[0].input.Length;
        var outputN = trainBoxedData[0].expected.Length;

        var testImageData = NImages.GetSmileNoiseImages(checkCount, imgSize.i, imgSize.j, smileSize, -1, 0.1, vectorizer, new Random(77));
        var testBoxedData = GetBoxed(testImageData);

        var options = new NOptions
        {
            Seed = 1,
            Topology = [inputN, 30, 30, 30, outputN],
            //UpTopology = [2, 4, 5, 6, 8, zN],
            //Activators = [NAct.Softmax, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB, NAct.SinB],
            LayerLinkFactors = [0.3, 0.9, 0.9, 0.9],
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
            EpochPerTrain = 1,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        };

        var trainer = new NTrainer(options.With(o=>o.TrainData = trainBoxedData));        
        trainer.Init();

        var file = @$"d:\\ai\train.txt";
        File.AppendAllText(file, "\r\n\r\n\r\n==================\r\n");

        void FileWriteLine(string message)
        {
            File.AppendAllText(file, $"{message}\r\n");
        }

        void SaveImages(NImageData imageData, double[][] outputs, Color color, string file)
        {
            outputs.ForEach((output, i) =>
            {                
                var info = imageData.images[i];
                var img = info.img.Clone();
                var result = (NValues.Unboxed(outputs[info.i][0], imageData.m, m), NValues.Unboxed(outputs[info.i][1], imageData.n, m));
                img.DrawRect(info.pos, rectSize, Color.Blue, 2);
                img.DrawRect(result, rectSize, color, 1);
                img.SaveAsBitmap(string.Format(file, info.i));
            });
        }
        
        for (var k = 1; k < epoch / options.EpochPerTrain; k++)
        {
            var state = await trainer.Train();

            if (state.errorChanged)
            {
                if (state.bestErrorChanged)
                {
                    var (avgError, avgDistance, trainOutputs) = state.ComputeBestModelDataset(trainBoxedData.Take(checkCount).ToArray(), errorScale);
                    var (avgTestError, avgTestDistance, testOutputs) = state.ComputeBestModelDataset(testBoxedData.Take(checkCount).ToArray(), errorScale);

                    var bestError = $"\r\nBestError: {state.bestError:E1} ({avgError:E1} - {avgTestError:E1}) ({avgDistance:F1}-{avgTestDistance:F1}) {state.CountInfo} {state.bestModel.TopologyInfo}";
                    Debug.WriteLine(bestError);
                    FileWriteLine(bestError);
                    FileWriteLine($"BestState: {state.bestModel.GetState().ToStateString()}");
                    SaveImages(trainImageData, trainOutputs, Color.DarkGreen, @"d:\\ai\trainImg\train{0}.bmp");
                    SaveImages(testImageData, testOutputs, Color.Red, @"d:\\ai\testImg\test{0}.bmp");
                    Debug.WriteLine($"Images refreshed");                
                }
                else
                    Debug.WriteLine($"Error: {state.bestError:E1} {state.CountInfo} {state.model.TopologyInfo}");
            }

            if (state.isUpChanged)
                Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().ToGraphString()}]");
        }

        return await Shapes.Cube.ToMotion();
    }    
}