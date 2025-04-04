﻿using System.Diagnostics;
using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel
{
    //private Queue<N> computeQueue = new();

    public double[] Predict(double[] vInput)
    {
        ComputeOutputs(vInput);

        return output.Select(n => n.f).ToArray();
    }

    private void SetInput(double[] vInput)
    {
        if (vInput.Length != input.Count)
            throw new InvalidInputDataException();

        input.Count.Range().ForEach(i => input[i].f = vInput[i]);
    }

    private void ComputeN(N n)
    {
        if (!n.isInput)
        {
            // compute output (f) from input (xx)
            n.f = n.act.Func(n);
        }

        // pass signal from output a to input b
        n.es.ForEach(e => e.b.xx += e.w * n.f);

        n.computed = true;
    }

    private void PreComputeN(N n)
    {
        if (!n.isInput)
        {
            // pre compute output (ff) from input (xx)
            n.ff = n.act.PreFunc(n);
        }

        n.preComputed = true;
    }

    public void ComputeTrainCase(double[] fs)
    {
        Queue<N> computeQueue = new();

        nns[blLv].ForEach(n => n.f = fs[n.ii]);

        // compute cleanup
        nns.Skip(blLv).ForEach(ns => ns.ForEach(n => { n.xx = 0; n.computed = false; n.preComputed = !n.act.IsLayerActivator; }));

        nns[blLv].ForEach(computeQueue.Enqueue);

        var counter = 10_000_000;

        while (computeQueue.TryDequeue(out var n))
        {
            if (counter-- == 0)
                throw new AlgorithmException("cannot compute");

            if (n.computed)
                continue;

            if (n.isInput || n.backEs.All(e => e.a.computed || e.a.believed))
            {
                if (n.act.IsLayerActivator)
                {
                    if (!n.preComputed)
                        PreComputeN(n);

                    if (n.act.layerPreComputed())
                    {
                        ComputeN(n);

                        // pass to compute b
                        n.es.ForEach(e => computeQueue.Enqueue(e.b));
                    }
                    else
                        computeQueue.Enqueue(n);
                }
                else
                {
                    ComputeN(n);

                    // pass to compute b
                    n.es.ForEach(e => computeQueue.Enqueue(e.b));
                }
            }
            else
                computeQueue.Enqueue(n);
        }
    }

    private bool isComputing = false;
    public void ComputeOutputs(double[] vInput)
    {
        if (isComputing)
            throw new NotImplementedException("Cannot compute same model in parallel");
        
        isComputing = true;

        Queue<N> computeQueue = new();
        SetInput(vInput);

        // compute cleanup
        ns.ForEach(n => { n.xx = 0; n.computed = false; n.preComputed = !n.act.IsLayerActivator; });

        input.ForEach(computeQueue.Enqueue);

        var counter = 10_000_000;

        while (computeQueue.TryDequeue(out var n))
        {
            if (counter-- == 0)
                throw new AlgorithmException("cannot compute");

            if (n.computed)
                continue;

            if (n.isInput || n.backEs.All(e => e.a.computed))
            {                
                if (n.act.IsLayerActivator)
                {
                    if (!n.preComputed)
                        PreComputeN(n);

                    if (n.act.layerPreComputed())
                    {
                        ComputeN(n);

                        // pass to compute b
                        n.es.ForEach(e => computeQueue.Enqueue(e.b));
                    }
                    else
                        computeQueue.Enqueue(n);
                }
                else
                {
                    ComputeN(n);

                    // pass to compute b
                    n.es.ForEach(e => computeQueue.Enqueue(e.b));
                }
            }
            else
                computeQueue.Enqueue(n);
        }

        isComputing = false;
    }
}
