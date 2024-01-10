
// Отложенные задачи, идеи проекта

#region Триангуляция
// Убрать композицию полигонов в алгоритм триангуляции (сортировка по y)

var fShape = new Fr[]
    {(-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1)};

var s = fShape.ToShape(3000, 0.02, indices: new[] { 0 }).ApplyColor(Color.Red);



var mb = Polygons.Polygon5;

var mb = MandelbrotFractalSystem.GetPoints(2, 0.003, 1000, 0.99).ToPolygon();

return /*mb.ToShape().ToNumSpots3(0.3, Color.Blue) +*/ mb.ToShape().ToLines(0.5, Color.Red);


var s1 = mb.ToShape().ToLines().ApplyColor(Color.Red);// + Shapes.Ball.Mult(0.1).ApplyColor(Color.Red);

Shape s;

try
{
    //    var ts = Triangulator.Triangulate(mb, 0.01);
    //    s = new Shape() { Points2 = mb.Points, Convexes = ts }.ApplyColor(Color.Red);
    //s = ts.SelectWithIndex((t,i)=>new Shape() {Points2 = mb.Points, Convexes = new []{t}}.MoveZ(0)).ToSingleShape();
    s = mb.ToTriangulatedShape(40, incorrectFix: 0)/*.Perfecto().ApplyZ(Funcs3Z.Hyperboloid)*/.ApplyColor(Color.Blue).ToLines(0.1, Color.Blue);//); 
}
catch (DebugException<(Shape polygon, Shape triangulation)> e)
{
    s = e.Value.triangulation.ToLines(0.2,
        Color.Blue); // + e.Value.polygon.ToMetaShape3(0.2, 0.2, Color.Green, Color.Red);
}
catch (DebugException<(Polygon p, int[][] t, Vector2[] ps)> e)
{
    s = new Shape() { Points2 = e.Value.p.Points, Convexes = e.Value.t }.ApplyColor(Color.Red)
        + e.Value.ps.ToPolygon().ToShape().ToNumSpots3(0.3, Color.Green)
        ;//+ mb.ToShape().ToSpots3(0.2, Color.Blue);
}

var s4 = net.Cut(mb.ToPolygon()).ToLines(0.5).ApplyColor(Color.Blue);

#endregion

#region 3d мандельброт
// Обойти поверхность мандельброта в 3d. Кубик со стороной d. Имеет соседей. У каждого кубика есть точка внутри множества и одна вне множества.
// Поверхность: множество соединенных точек принадлежащих кубикам

// todo: Surfer доделать

// SceneMotion:

Func<Vector3, bool> solidFn = v => v.x.Pow2() + v.y.Pow2() + v.z.Pow2() < 1;
//Func<Vector3, bool> solidFn = v => MandelbrotQuaternionFractalSystem.CheckBounds(new Model4D.Quaternion(v.x, v.y, v.z, 0), 1000);
var ss = Surfer.FindSurface(solidFn, 0.02);

return new[]
{
    ss/*.ToSpots3(0.8)*/.ApplyColor(Color.Blue),
    //MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().ToSpots3().ApplyColor(Color.Red),
    Shapes.CoodsWithText
}.ToSingleShape().ToMotion();

var n = 50;
double Fn(int k) => 3.0 * k / (n - 1) - 1.5;

var vs = (n, n, n).SelectRange((a, b, c) => new Model4D.Quaternion(
        Fn(a),
        Fn(b),
        Fn(c),
        0
    ))
    .Where(q => MandelbrotQuaternionFractalSystem.CheckBounds(q, 1000)).ToArray();

var point = Shapes.Dodecahedron.Mult(0.04);

var s = vs.Select(v => point.Move(v.x, v.y, v.z)).ToSingleShape();

return new[]
{
    s.ApplyColor(Color.Blue),
    MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().ToSpots3().ApplyColor(Color.Red),
    Shapes.CoodsWithText
}.ToSingleShape().ToMotion();

#endregion

#region триангуляция с доп. точками для деформаций
public Task<Motion> Scene()
{
    //var mb = Polygons.Polygon5;

    var mb = MandelbrotFractalSystem.GetPoints(2, 0.003, 1000, 0.99).ToPolygon().Centered();
    var net = Parquets.Triangles(20, 40).ToShape3().Perfecto(2.1).ToShape2();
    var sIn = net.CutInside(mb).Points.ToPolygon();
    var prs = sIn.ToPerimeter();
    var newMb = prs.Aggregate(mb, (a, b) => a.Join(b));
    //var sOut = net.CutOutside(mb).ToShape3().ToLines(0.5).ApplyColor(Color.Blue);

    return new[]
    {
            newMb.ToShape(/*triangulate:true*/).ToLines(0.5, Color.Red),
            sIn.ToShape2().ToShape3().ToLines(0.5).ApplyColor(Color.Blue),
            //sOut.MoveZ(-0.1).ToMeta()
        }.ToSingleShape().ToMotion();
}
#endregion

