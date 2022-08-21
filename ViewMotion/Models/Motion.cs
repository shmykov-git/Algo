using System;
using System.Threading.Tasks;
using Model;

namespace ViewMotion.Models;

class Motion
{
    public double CameraDistanceCoff = 1;
    public Shape? Shape;
    public Func<int, Action<Shape>, Task<bool>> Step;
}