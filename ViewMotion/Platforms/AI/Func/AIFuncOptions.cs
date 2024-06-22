namespace ViewMotion.Platforms.AI.Func;

public class AIFuncOptions : AIShowOptions
{
    public double m = 0.75;
    public int trainN = 20;
    public (double from, double to) trainR = (-2, 2);
    public int modelN = 50;
    public (double from, double to) modelR = (-2 / 0.75, 2 / 0.75);
}
