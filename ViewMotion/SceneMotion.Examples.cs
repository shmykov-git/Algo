﻿using System.Drawing;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model3D.Extensions;
using Model3D.Systems;
using Model3D.Systems.Model;
using ViewMotion.Extensions;
using ViewMotion.Models;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> IllBeBack()
    {
        var s0 = vectorizer.GetText("I'll be back", 300).Perfecto(10).ApplyColor(Color.Blue);

        var frames = WaterSystem.IllBeBackMotion(new WaterCubeOptions()
        {
            SceneSize = new Vector3(16, 16, 16),
            WaterEnabled = true,
            WaterPosition = new Vector3(0, -1.5, 0),
            WaterDir = new Vector3(-0.06, 1, 0.06),
            WaterSpeed = 0.16,

            SceneMotionSteps = 300,
            ParticlePerEmissionCount = 2,
            EmissionAnimations = 1,
            StepAnimations = 10,
        });

        return frames.ToMotion(s0);
    }

    public Task<Motion> Fountain()
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
            PlatformColor = Color.DarkGreen,
            FountainColor = Color.Gray,
        });

        return frames.ToMotion(s0);
    }
}