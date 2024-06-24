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
        //var data = NImages.GetSmileNoiseImages(100, 64, 64, 25, -1, 0.1, vectorizer, new Random(77));
        //var vv = data.images.Select(img => img.img.ApplySumFilter(NImage.ToBit)).ToArray();
        //vv.ForEach((v, i) => v.SaveAsBitmap(@$"d:\\ai\tmp\sobel{i}.png"));

        var m = 0.75;
        (int i, int j) imgSize0 = (400, 400);
        int trainCount = 100;
        int checkCount = 100;
        int saveCount = 3;
        var smileSize = 25;
        var rectSize = (21, 21);
        var errorScale = Math.Max(imgSize0.i, imgSize0.j) / m;

        var epoch = 10000;

        NBoxed[] GetBoxed(IEnumerable<NImageInfo> images, List<NImageInfo> list, int fillCount) => images.Select(v =>
        {
            if (fillCount-- > 0)
            {
                double[] expected = [NValues.Boxed(v.pos.i, v.img.m, m), NValues.Boxed(v.pos.j, v.img.n, m)];
                list.Add(new NImageInfo { i = v.i, img = v.img.Clone(), pos = v.pos });
            }

            if (v.i % 100 == 0 && v.i > 0)
                Debug.WriteLine($"Generated images: {v.i} | {DateTime.Now}");

            return new NBoxed
            {
                i = v.i,
                input = v.img
                .ApplySumFilter(3, NImage.ToBit, NFuncs.EachN0(1))
                .ApplySumFilter(3, NImage.AsIs, NFuncs.EachN0(1))
                .ApplySumFilter(3, NImage.AsIs, NFuncs.EachN0(1))
                .ApplySumFilter(3, NImage.AsIs, NFuncs.EachN0(8))
                .pixels.Select(c => NValues.Boxed(c, 81, m)).ToArray(),
                expected = [NValues.Boxed(v.pos.i, v.img.m, m), NValues.Boxed(v.pos.j, v.img.n, m)]
            };
        }).ToArray();

        Debug.WriteLine($"Start creating train images: {DateTime.Now}");
        List<NImageInfo> trainSaveImages = new();
        var trainImages = NImages.GetSmileNoiseImages(trainCount, imgSize0.i, imgSize0.j, smileSize, -1, 0.1, vectorizer, rnd);
        var trainBoxedData = GetBoxed(trainImages, trainSaveImages, saveCount);
        var inputN = trainBoxedData[0].input.Length;
        var outputN = trainBoxedData[0].expected.Length;
        Debug.WriteLine($"End creating train images: {DateTime.Now}");

        List<NImageInfo> testSaveImages = new();
        var testImages = NImages.GetSmileNoiseImages(checkCount, imgSize0.i, imgSize0.j, smileSize, -1, 0.1, vectorizer, new Random(77));
        var testBoxedData = GetBoxed(testImages, testSaveImages, saveCount);

        var options = new NOptions
        {
            Seed = 1,
            Topology = [inputN, 16, 16, 16, outputN],
            LayerLinkFactors = [0.2, 0.9, 0.9, 0.9],
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

        void SaveImages(List<NImageInfo> images, double[][] outputs, Color color, string file)
        {
            images.ForEach((info, i) =>
            {                
                var output = outputs[i];
                var img = info.img.Clone();
                var result = (NValues.Unboxed(outputs[info.i][0], info.img.m, m), NValues.Unboxed(outputs[info.i][1], info.img.n, m));
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

                    var bestError = $"\r\nBestError: {state.bestError:E1} ({avgError:E1} - {avgTestError:E1}) ({avgDistance:F1}-{avgTestDistance:F1}) {state.CountInfo} {state.bestModel.TopologyInfo} | {DateTime.Now}";
                    Debug.WriteLine(bestError);
                    FileWriteLine(bestError);
                    FileWriteLine($"BestState: {state.bestModel.GetState().ToStateString()}");
                    SaveImages(trainSaveImages, trainOutputs, Color.DarkGreen, @"d:\\ai\trainImg\train{0}.bmp");
                    SaveImages(testSaveImages, testOutputs, Color.Red, @"d:\\ai\testImg\test{0}.bmp");
                    Debug.WriteLine($"Images refreshed");                
                }
                else
                    Debug.WriteLine($"Error: {state.bestError:E1} {state.CountInfo} {state.model.TopologyInfo} | {DateTime.Now}");
            }

            if (state.isUpChanged)
                Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().ToGraphString()}]");
        }

        return await Shapes.Cube.ToMotion();
    }    
}