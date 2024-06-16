using System;
using System.Threading.Tasks;
using Meta.Extensions;
using Model;

namespace ViewMotion.Models;

class MotionOptions
{
    public event Func<InteractType, object, Task> OnInteract;

    public void Interact(InteractType type, object? arg = null)
    {
        OnInteract.RaiseNoWaitAsync(type, arg);
    }

    public CameraMotionOptions CameraMotionOptions { get; set; }
    public double? CameraDistance { get; set; }
    public Shape StartShape { get; set; } 
    public TimeSpan? StepDelay { get; set; }
}