using Aspose.ThreeD.Utilities;

namespace Model3D.Systems
{
    public class FountainOptions
    {
        public Vector3 SceneSize = new Vector3(20, 20, 20);
        public (int m, int n) SceneSteps = (1, 1);
        public int ParticlePerEmissionCount = 2;
        public int ParticleCount = 5000;
        public double ParticleRadius = 0.1;
        public Vector3 ParticleSpeed = new Vector3(0.002, 0.12, 0.004);
        public double NetSize = 0.25;
        public Vector3 Gravity = new Vector3(0, -1, 0);
        public double GravityPower = 0.001;
        public double LiquidPower = 0.0001;
        public int Seed = 0;
        public int SkipAnimations = 1500;
        public int EmissionAnimations = 1;
        public int StepAnimations = 300;
        public int? StepDebugNotify = 50;
        public bool JustAddShamrock = true;
    }
}