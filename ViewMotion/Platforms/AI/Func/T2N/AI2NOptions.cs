using System;
using System.Drawing;
using Model;
using ViewMotion.Platforms.AI.Func;

namespace ViewMotion.Platforms.AI.Func.T2N;

public class AI2NOptions : AIFuncOptions
{
    public Func<int, int> shapeType = k => k % 99 / 33;
    public P2NMode mode = P2NMode.Learn;
    public Shape learnShape;
    public int zN;
    public bool AllowNullZ = true;
    public Color[] colors = [Color.DarkSeaGreen, Color.Maroon, Color.DarkTurquoise, Color.DarkSalmon];
}
