namespace Model3D.Tools.Model;

public static class BezierValues
{
    public static BezierOptions HighLetterOptions => new BezierOptions // 1000x1000
    {
        ColorLevel = 150,
        SmoothingResultLevel = 7,
        SmoothingAlgoLevel = 7,
        MinPointDistance = 7,
        MaxPointDistance = 50,
        AnglePointDistance = 7,
    };

    public static BezierOptions PerfectLetterOptions => new BezierOptions // 500x500
    {
        ColorLevel = 150,
        SmoothingResultLevel = 6,
        SmoothingAlgoLevel = 6,
        MinPointDistance = 6,
        MaxPointDistance = 30,
        AnglePointDistance = 6,
    };

    public static BezierOptions MediumLetterOptions => new BezierOptions // 300x300
    {
        ColorLevel = 150,
        SmoothingResultLevel = 5,
        SmoothingAlgoLevel = 5,
        MinPointDistance = 5,
        MaxPointDistance = 20,
        AnglePointDistance = 5,
    };

    public static BezierOptions LowLetterOptions => new BezierOptions // 200x200
    {
        ColorLevel = 150,
        SmoothingResultLevel = 3,
        SmoothingAlgoLevel = 3,
        MinPointDistance = 3,
        MaxPointDistance = 15,
        AnglePointDistance = 3,
    };

    public static BezierOptions ExtremeLetterOptions => new BezierOptions
    {
        ColorLevel = 150,
        SmoothingResultLevel = 1,
        PolygonPointStrategy = PolygonPointStrategy.Circle,
        SmoothingAlgoLevel = 3,
        MinPointDistance = 3,
        MaxPointDistance = 9,
        AnglePointDistance = 3,
    };
}
