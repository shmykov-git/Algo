using Aspose.ThreeD.Utilities;

namespace Model3D.Systems.Model
{
    public class WaterfallOptions
    {
        public Vector3 SceneSize = new Vector3(12, 15, 12);
        public (int m, int n) SceneSteps = (4, 4);
        public int ParticleCount = 500;
        public double ParticleRadius = 0.1;
        public double NetSize = 0.25;
        public double GutterCurvature = 1; // from 0 to 2
        public Vector3 GutterOffset = new Vector3(0, 0, 0);
        public Vector3 GutterRotation = new Vector3(0, 6, 1);
        public Vector3 SphereOffset = new Vector3(0, 0, 0);
        public double SphereRadius = 3;
        public Vector3 WatterOffset = new Vector3(0, 0, 0);
        public Vector3 Gravity = new Vector3(0, -1, 0);
        public double GravityPower = 0.001;
        public double LiquidPower = 0.0001;
        public int Seed = 0;
        public int SkipAnimations = 0;
        public int StepAnimations = 40;
        public int? StepDebugNotify = 50;
    }
}