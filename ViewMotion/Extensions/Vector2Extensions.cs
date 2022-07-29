using System.Windows;
using Model;

namespace ViewMotion.Extensions;

static class Vector2Extensions
{
    public static Point ToP2D(this Vector2 v) => new(v.x, v.y);
}