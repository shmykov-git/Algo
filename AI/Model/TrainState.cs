using AI.NBrain;

namespace AI.Model;

public class TrainState
{
    public int epoch;
    public int trainCount;
    public int computeCount;

    public double error = double.MaxValue;
    public bool errorChanged;
    public NModel model;

    public double bestError = double.MaxValue;
    public NModel bestModel;
    public bool bestErrorChanged;

    public bool isUp;
    public bool isUpChanged;
    public bool isLevelUp;
    public int upLevelChangesCount;
    public int epochToGrowUp;

    public DateTime t0;
    public DateTime t;
    public TimeSpan time => t - t0;

    public string CountInfo => $"[{trainCount}-{epoch}-{computeCount}]";
    public string ModelInfo => $"Error: {error:E1}{(model.blLv > 0 ? $" bl={model.blLv}" : "")}";
    public string BestModelInfo => $"BestError: {bestError:E1}";
}
