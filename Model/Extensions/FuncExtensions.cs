using System;

namespace Model.Extensions;

public static class FuncExtensions
{
    public static Func<TArg, double> Minus<TArg>(this Func<TArg, double> fn) => x => -fn(x);
}