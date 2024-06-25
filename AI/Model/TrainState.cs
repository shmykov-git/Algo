using AI.NBrain;
using Model.Extensions;
namespace AI.Model;

public class TrainState
{
    public int epoch;
    public bool epochChanged;
    public int trainCount;
    public int computeCount;

    public double trainError = double.MaxValue;
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
    public string ModelInfo => $"Error: {trainError:E1}{(model.blLv > 0 ? $" bl={model.blLv}" : "")}";
    public string BestModelInfo => $"BestError: {bestError:E1}";
    public string BestModelDInfo(double scale) => $"BestError: {bestError:E1} ({scale * bestModel.trainDistance:F2})";

    public (double error, double distance, double[][] outputs) ComputeBestModelDataset(NBoxed[] data, double distanceScale = 1)
    {
        var outputs = data.Select(t => 
        { 
            bestModel.ComputeOutputs(t.input); 
            return bestModel.output.Select(n => n.f).ToArray(); 
        }).ToArray();

        var sumError = data.Select(t => (outputs[t.i], t.expected).SelectBoth((x1, x2) => (x2 - x1).Pow2()).Sum()).Sum();
        var sumDistance = data.Select(t => (outputs[t.i], t.expected).SelectBoth((x1, x2) => (x2 - x1).Abs()).Sum()).Sum();
        var avgError = sumError / (data.Length * bestModel.output.Count);
        var avgDistance = distanceScale * sumDistance / (data.Length * bestModel.output.Count);

        return (avgError, avgDistance, outputs);
    }
}
