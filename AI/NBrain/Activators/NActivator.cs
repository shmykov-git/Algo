using AI.Libraries;
using AI.Model;
using Model.Extensions;
using static Model3D.Actives.ActiveWorld;

namespace AI.NBrain.Activators;

/// <summary>
/// Метод обратного распространения ошибки
/// https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%BE%D0%B3%D0%BE_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%BE%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F_%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8
/// </summary>
public abstract class NActivator
{
    protected NModel model;
    public bool IsLayerActivator = false;
    protected int lv;
    public NFunc PreFunc;
    public Func<bool> layerPreComputed = () => true;
    public NAct act => model.options.Activator;

    public NFunc Func;
    public NFunc DerFunc;

    protected double alfa;
    protected double nu;
    protected double power;
    protected double a;


    public NActivator(NModel model)
    {
        this.model = model;
        
        alfa = model.options.Alfa;
        nu = model.options.Nu;
        power = model.options.Power;

        a = alfa * power;
    }
}
