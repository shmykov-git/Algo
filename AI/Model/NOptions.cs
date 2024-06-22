using AI.Libraries;

namespace AI.Model;
public class NOptions
{
    public int Seed { get; set; } = 0;                                  // use the same randomizing process to repeat creation and learning
    public double ShaffleFactor { get; set; } = 0.1;                    // shaffle percent of training data for each learn circle
    public bool CleanupTrainTails { get; set; } = false;                // clean some training data for each training (default = false)
    public (int i, int j, double w)[][] Model { get; set; } = [];       // Learn model (default if set)
    public (int i, int j)[][] Graph { get; set; } = [];                 // start topology graph (default if set)
    public (int i, int j)[][] UpGraph { get; set; } = [];               // top topology graph to grow (default if set)
    public int[] Topology { get; set; } = [2, 5, 1];                    // start topology
    public int[] UpTopology { get; set; } = [2, 5, 5, 1];               // top topology to grow
    public bool AllowGrowing { get; set; } = false;                     // use net growing
    public (double a, double b) Weight0 { get; set; } = (-0.05, 0.05);  // N.w0 = a + rnd * (b - a)
    public (double a, double b)? PowerWeight0 { get; set; } = null;     // N.w0 = (a + rnd * (b - a)) / power (default if set)
    public NAct Activator { get; set; } = NAct.Sigmoid;                       // activator function
    public NAct[] Activators { get; set; } = [];                         // layer activator functions
    public double ActBias { get; set; } = 0.01;                         // SinA activator value
    public bool AllowBelief { get; set; } = false;      // use maximum number of top levels to learn
    public int BeliefDeep { get; set; } = 3;            // maximum number of top levels to learn
    public double Power { get; set; } = 100;            // use 'x * power' instead of x
    public double SymmetryFactor { get; set; } = 0;     // 0 - off, 1 - e.dw should be avg for all training data
    public int ParallelCount { get; set; } = 1;         // not implemented, try to use parallel calculations
    public double LinkFactor { get; set; } = 0.2;       // percent of linked level edges
    public double[] LayerLinkFactors { get; set; } = [];// percent of linked level edges per layers
    public double CrossLinkFactor { get; set; } = 0;    // not implemented

    public double Nu { get; set; } = 0.1;               // backpropagation parameter
    public double Alfa { get; set; } = 0.5;             // backpropagation and power multiplicator parameter

    public double DynamicW0Factor { get; set; } = 0.1;  // set w when new E added as avg sibling E multiplicator

    public (int num, double[] input, double?[] expected)[] TrainData { get; set; }    // training data to learn
    public int EpochPerTrain { get; set; } = 200;               // number of epochs (train data calculation circles)
    public int EpochUnwanted { get; set; } = 300;               // number of epochs to reach zero weight before remove it
    public int EpochBeforeGrowing { get; set; } = 10_000;       // number of epochs before graph start to grow
    public int EpochAfterLevelGrowUp { get; set; } = 10_000;    // number of epochs when graph reaches next level
    public int EpochAfterGrowUp { get; set; } = 1_000;          // number of epochs when graph has any changes (new edge added)
}
