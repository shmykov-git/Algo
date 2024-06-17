using AI.Libraries;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain.Activators;

/// <summary>
/// Метод обратного распространения ошибки
/// https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%BE%D0%B3%D0%BE_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%BE%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F_%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8
/// </summary>
public class NActivator
{
    public NFunc Func;
    public NDerFunc DerFunc;
    public NFunc dampingFn;

    protected double alfa;
    protected double nu;
    protected double power;
    protected double a;

    public NActivator(NOptions options)
    {
        alfa = options.Alfa;
        nu = options.Nu;
        power = options.Power;
        a = alfa * power;
        dampingFn = NFuncs.GetDampingFn(options.DampingCoeff);
    }
}
