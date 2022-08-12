
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using ViewMotion.Extensions;
using ViewMotion.Models;

namespace ViewMotion;

class SceneMotion
{
    #region ctor

    private readonly Vectorizer vectorizer;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
    }

    #endregion

    public Task<Motion> Scene()
    {
        var s0 = vectorizer.GetText("Fountain").Perfecto(10).ApplyColor(Color.Blue);

        var frames = WaterSystem.FountainMotion(new FountainOptions()
        {
            SceneSize = new Vector3(12, 18, 12),
            ParticleCount = 10000,
            ParticlePerEmissionCount = 2,
            EmissionAnimations = 1,
            ParticleSpeed = new Vector3(0.002, 0.12, 0.004),
            WaterPosition = new Vector3(0, 0.3, 0),
            Gravity = new Vector3(0, -1, 0),
            GravityPower = 0.001,
            LiquidPower = 0.0001,
            SkipAnimations = 0,
            StepAnimations = 10,
            SceneMotionSteps = 1000,
            JustAddShamrock = false,
            PlatformColor = Color.DarkGreen
        });

        return frames.ToMotion(s0);
    }
}