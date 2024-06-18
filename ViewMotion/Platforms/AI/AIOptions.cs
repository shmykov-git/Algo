using System;
using System.Drawing;
using Model;
using Model3D.Libraries;

namespace ViewMotion.Platforms.AI;

public class AIShowOptions
{
    public int frames = 2000;
    public bool showTopology = true;
    public bool showTopologyWeights = true;
    public bool showError = true;
    public bool showTime = true;
    public bool topologyNums = false;
    public bool topologyWeightNums = false;
}

public class AIOptions : AIShowOptions
{
    public double m = 0.75;
    public int trainN = 20;
    public (double from, double to) trainR = (-2, 2);
    public int modelN = 50;
    public (double from, double to) modelR = (-2 / 0.75, 2 / 0.75);
    public Func<int, bool> withTrain = k => k % 100 < 50;
}

public class AI21Options: AIOptions
{
    public P21Mode mode = P21Mode.Learn;
    public SurfaceFunc learnFunc;
}

public class AI2NOptions: AIOptions
{
    public P2NMode mode = P2NMode.Learn;
    public Shape learnShape;
    public int zN;
    public Color[] colors = [Color.Red, Color.Maroon, Color.Navy, Color.Peru];
}
