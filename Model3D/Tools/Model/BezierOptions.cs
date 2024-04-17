using Vector2 = Model.Vector2;

namespace Model3D.Tools.Model
{
    public class BezierOptions : BitmapOptions
    {
        public int MinPointDistance { get; set; } = 5;
        public int MaxPointDistance { get; set; } = 16;
        public int SmoothingResultLevel { get; set; } = 3;
        public int SmoothingAlgoLevel { get; set; } = 3;

        public bool DebugProcess { get; set; }

        public Vector2[] bps;
        public Vector2[] ps;
    }
}