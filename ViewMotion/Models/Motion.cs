using System;
using System.Threading.Tasks;
using Model;

namespace ViewMotion.Models;

class Motion
{
    public Shape Shape;
    public Func<int, Action<Shape>, Task> Step;
}