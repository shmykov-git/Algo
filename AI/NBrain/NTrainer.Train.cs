using System.Diagnostics;
using System.Text.RegularExpressions;
using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NTrainer
{
    private double[][] blMatrix = null;
    private bool needBlFill = false;
    bool isLevelUp;
    private TrainState state = new();
    
    public async Task<TrainState> Train()
    {
        if (state.epoch == 0)
            state.t0 = DateTime.Now;

        state.error = double.MaxValue;
        state.errorChanged = false;
        state.bestErrorChanged = false;
        state.isUpChanged = false;
        state.isLevelUp = false;
        
        for (var i = 0; i < options.EpochPerTrain; i++)
        {
            if (options.AllowGrowing)
                TrainWithGrowUp();

            var error = await TrainByTrainingData();
            state.epoch++;

            if (error < state.error)
            {
                state.error = error;
                state.errorChanged = true;
                state.model = model.Clone();

                if (error < state.bestError)
                {
                    state.bestError = error;
                    state.bestErrorChanged = true;
                    state.bestModel = model;
                }
            }
        }

        state.trainCount++;

        return new TrainState
        {
            epoch = state.epoch,
            trainCount = state.trainCount,
            computeCount = state.computeCount,
            error = state.error,
            model = state.model,
            errorChanged = state.errorChanged,
            bestError = state.bestError,
            bestModel = state.bestModel,
            bestErrorChanged = state.bestErrorChanged,
            isUp = state.isUp,
            isUpChanged = state.isUpChanged,
            isLevelUp = state.isLevelUp,
            upLevelChangesCount = state.upLevelChangesCount,
            epochToGrowUp = state.epochToGrowUp,
            t0 = state.t0,
            t = DateTime.Now
        };
    }

    private void MakeLevelBelieved()
    {
        if (model.maxLv >= options.BeliefDeep)
            model.MakeBelieved(model.maxLv - options.BeliefDeep + 1);
    }

    private void InitBeliefMatrix()
    {
        var lv = model.blLv;

        if (blMatrix != null
            && model.nns[lv].Count == blMatrix[0].Length)
            return;

        blMatrix = (options.TrainData.Length).Range().Select(_ => (model.nns[lv].Count).Range(_ => 0.0).ToArray()).ToArray();
        needBlFill = true;
    }

    private void TrainWithGrowUp()
    {
        if (state.epoch == 0)
            state.epochToGrowUp = options.EpochBeforeGrowing;

        if (state.epochToGrowUp-- == 0)
        {
            if (isLevelUp && options.AllowBelief)
                MakeLevelBelieved();

            (var isUp, isLevelUp) = GrowUp();
            
            state.epochToGrowUp = state.upLevelChangesCount > 0
                ? (isLevelUp ? options.EpochAfterLevelGrowUp : options.EpochAfterGrowUp)
                : 0;
            
            if (isUp)
            {
                state.isUp = true;
                state.isUpChanged = true;
                state.epochToGrowUp = -1;
            }

            if (isLevelUp)
                state.isLevelUp = true;
        }
    }

    private async Task<double> TrainByTrainingData()
    {
        if (options.AllowBelief)
            InitBeliefMatrix();

        var data = options.TrainData.ToArray();
        var pN = options.ParallelCount;
        var pNs = (pN).Range().ToArray();
        NModel[] models = [model];
        var es = model.es.ToArray();
        var symmetryLen = (int)Math.Round(options.SymmetryFactor * data.Length / pN);
        
        if (symmetryLen == 0)
            symmetryLen = 1;

        double ModelStep(int mI, (int num, double[] input, double[] expected) t, int k)
        {
            if (mI != 0 && k % symmetryLen == 0)
            {
                models[mI].es.ForEach((e, i) =>
                {
                    Interlocked.Exchange(ref es[i].sumDw, e.sumDw);
                    e.sumDw = 0;
                });
            }
            
            return model.blLv > 0 
                ? TrainCase(models[mI], t.num, blMatrix[t.num], t.expected)
                : TrainCase(models[mI], t.num, t.input, t.expected);
        }

        if (options.ShaffleFactor > 0)
            data.Shaffle((int)(options.ShaffleFactor * (3 * data.Length + 7)), rnd);

        if (options.CleanupTrainTails)
            CleanupTrainTails();

        if (data.Length % options.ParallelCount != 0)
            throw new NotImplementedException();


        double sumError = 0;

        for(var k =0; k< data.Length / pN; k++)
        {
            if (k % symmetryLen == 0)
            {
                model.es.ForEach(e =>
                {
                    e.dw = e.sumDw / (symmetryLen * pN);
                    e.sumDw = 0;
                });
            }

            if (pN == 1)
            {
                sumError += ModelStep(0, data[k], k);
            }
            else
            {
                throw new NotImplementedException();
                // медленнее в 100!! раз, todo: model of n for train matrix
                //models = (pN).Range().Select(i => i == 0 ? model : model.Clone()).ToArray();
                //var errors = await pNs.SelectInParallelAsync(j => ModelStep(j, data[k * pN + j], k));
                //sumError += errors.Sum();
            }
        }

        needBlFill = false;

        return sumError / data.Length;
    }

    private double TrainCase(NModel model, int num, double[] tLayerInput, double[] tExpected)
    {
        if (tExpected.Length != options.Topology[^1])
            throw new InvalidExpectedDataException();

        model.ComputeTrainCase(tLayerInput);
        state.computeCount++;

        if (needBlFill && options.AllowBelief)
            model.nns[model.blLv].ForEach(n => blMatrix[num][n.ii] = n.f);
        
        // learn cleanup
        Queue<N> learnQueue = new Queue<N>(model.unbelievedCapacity);

        // skip no compute nodes to finish learn process in any cases
        model.unbelievedNs.ForEach(n => { n.learned = n.es.Count == 0; });

        model.output.ForEach((n, i) => 
        {
            LearnBackPropagationOutput(n, tExpected[i]);
            n.learned = true;
        });

        model.output.SelectMany(n => n.backEs.Select(e => e.a)).Distinct().ForEach(learnQueue.Enqueue);

        var counter = 10000;

        while (learnQueue.TryDequeue(out var n))
        {
            if (counter-- == 0)
                throw new AlgorithmException("cannot learn");

            if (n.learned || n.believed)
                continue;

            if (n.es.All(e => e.b.learned))
            {
                LearnBackPropagation(n);
                n.learned = true;
                n.backEs.Select(e => e.a).ForEach(learnQueue.Enqueue);
            }
            else
                learnQueue.Enqueue(n);
        }

        var err = 0.5f * model.output.Select((n, i) => (tExpected[i] - n.f).Pow2()).Sum();
        model.error = err;

        return err;
    }
}
