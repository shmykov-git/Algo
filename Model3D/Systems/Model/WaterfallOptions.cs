using System.Drawing;

namespace Model3D.Systems.Model
{
    public class WaterfallOptions : WaterCubeOptions
    {
        public double GutterCurvature = 1; // from 0 to 2
        public Vector3 GutterOffset = new Vector3(0, 0, 0);
        public Vector3 GutterRotation = new Vector3(0, 6, 1);
        public Vector3 SphereOffset = new Vector3(0, 0, 0);
        public double SphereRadius = 3;
        public Vector3 WatterOffset = new Vector3(0, 0, 0);
        public Color GutterColor = Color.Black;
        public Color SphereColor = Color.Black;

        public WaterfallOptions()
        {
            SceneSize = new Vector3(12, 15, 12);
            SceneSteps = (4, 4);
            SceneMotionSteps = 500;
            ParticleCount = 500;
            ParticleRadius = 0.1;
            NetSize = 0.25;
            Gravity = new Vector3(0, -1, 0);
            GravityPower = 0.001;
            LiquidPower = 0.0001;
            Seed = 0;
            SkipAnimations = 0;
            StepAnimations = 40;
            StepDebugNotify = 50;
        }
    }
}