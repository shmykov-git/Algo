using System.Drawing;
using Model3D.AsposeModel;
using Model.Interfaces;

namespace Model3D.Systems.Model
{
    public class WaterCubeOptions
    {
        public Vector3 SceneSize = new Vector3(12, 18, 12);
        public (int m, int n) SceneSteps = (1, 1);
        public int SceneMotionSteps = 1000;
        public int ParticleInitCount = 100;
        public int ParticlePerEmissionCount = 2;
        public int ParticleCount = 5000;
        public double ParticleRadius = 0.1;
        public Vector3 ParticleSpeed = new Vector3(0.002, 0.12, 0.004);
        public double NetSize = 0.25;
        public Vector3 Gravity = new Vector3(0, -1, 0);
        public double GravityPower = 0.001;
        public double LiquidPower = 0.0001;
        public double FrictionFactor = 0.5;
        public bool WaterEnabled = false;
        public double WaterSpeed = 0.14;
        public Vector3 WaterDir = new Vector3(0.04, 1.5, 0.04);
        public Vector3 WaterPosition = new Vector3(0, 0, 0);
        public int Seed = 0;
        public int SkipAnimations = 0;
        public int EmissionAnimations = 1;
        public int StepAnimations = 200;
        public int? StepDebugNotify = 50;
        public double ParticlePlaneBackwardThikness = 4;
        public double ParticleMaxMove = 2;
        public PlatformType PlatformType = PlatformType.Square;
        public double? PlatformSize = null;
        public Color PlatformColor = Color.Black;
        public bool IsReverseReplay { get; set; } = true;
    }

    public enum PlatformType
    {
        Square,
        Circle,
        Heart,
        Mandelbrot
    }
}