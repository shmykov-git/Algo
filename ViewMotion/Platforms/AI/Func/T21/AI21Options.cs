using Model3D.Libraries;
using System;

namespace ViewMotion.Platforms.AI.Func.T21;

public class AI21Options : AIFuncOptions
{
    public Func<int, bool> withTrain = k => k % 100 < 50;
    public P21Mode mode = P21Mode.Learn;
    public SurfaceFunc learnFunc;
}
