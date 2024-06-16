using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;
public partial class NTrainer
{
    private double[][] blMatrix = null;
    private bool needBlFill = false;
    //private int trainCaseNum = -1;

    public void MakeBelieved()
    {
        if (!options.AllowBelief)
            throw new NotAllowedException("UseLayerBelief");

        if (model.maxLv >= options.BeliefDeep)
            model.MakeBelieved(model.maxLv - options.BeliefDeep);
    }

    private void InitTrainMatrix()
    {
        var lv = model.blLv;

        if (blMatrix != null 
            && model.nns[lv].Count == blMatrix[0].Length)
            return;

        blMatrix = (options.Training.Length).Range().Select(_ => (model.nns[lv].Count).Range(_ => 0.0).ToArray()).ToArray();
        needBlFill = true;
    }

    public async Task<double> Train()
    {
        if (options.AllowBelief)
            InitTrainMatrix();

        var data = options.Training.ToArray();
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
        model.avgDelta = model.ns.Average(n => n.delta.Abs());

        return err;
    }
}
