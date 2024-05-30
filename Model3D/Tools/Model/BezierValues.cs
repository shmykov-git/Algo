namespace Model3D.Tools.Model;

public static class BezierValues
{
    public static BezierOptions HighLetterOptions => new BezierOptions
    {
        ColorLevel = 150,
        SmoothingResultLevel = 5,
        SmoothingAlgoLevel = 5,
        MinPointDistance = 5,
        MaxPointDistance = 50,
        AnglePointDistance = 5,
    };

    public static BezierOptions PerfectLetterOptions => new BezierOptions
    {
        ColorLevel = 150,
        SmoothingResultLevel = 5,
        SmoothingAlgoLevel = 5,
        MinPointDistance = 5,
        MaxPointDistance = 30,
        AnglePointDistance = 5,
    };

    public static BezierOptions MediumLetterOptions => new BezierOptions
    {
        ColorLevel = 150,
        SmoothingResultLevel = 5,
        SmoothingAlgoLevel = 5,
        MinPointDistance = 5,
        MaxPointDistance = 20,
        AnglePointDistance = 5,
    };

    public static BezierOptions LowLetterOptions => new BezierOptions
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
        SmoothingAlgoLevel = 3,
        MinPointDistance = 3,
        MaxPointDistance = 9,
        AnglePointDistance = 3,
    };
}
