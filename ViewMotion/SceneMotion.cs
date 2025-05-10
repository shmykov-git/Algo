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
using Quaternion = Model3D.AsposeModel.Quaternion;
using Vector2 = Model.Vector2;
using Vector3 = Model3D.AsposeModel.Vector3;
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
using View3D;
using Model3D.Tools.Vectorization;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        return BulletThrowMotion();
    }
}