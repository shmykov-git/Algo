using System;
using ViewMotion.Models;

namespace ViewMotion.Extensions;

static class LightOptionsExtensions
{
    public static LightOptions With(this LightOptions options, Action<LightOptions> modifyFn)
    {
        var copy = new LightOptions()
        {
            Color = options.Color,
            Direction = options.Direction,
            LightType = options.LightType
        };

        modifyFn(copy);

        return copy;
    }
}