using Model3D.Extensions;
using Model3D.Tools;
using System.Drawing;
using Shape = Model.Shape;

namespace View3D;

partial class Scene
{
    #region ctor

    private readonly StaticSettings staticSettings;
    private readonly Vectorizer vectorizer;

    public Scene(StaticSettings staticSettings, Vectorizer vectorizer)
    {
        this.staticSettings = staticSettings;
        this.vectorizer = vectorizer;
    }

    #endregion

    public Shape GetShape() => vectorizer.GetText("Use scene motion for any scene", 300).Perfecto().ScaleZ(0.1).ApplyColor(Color.Blue); // SceneMotion.cs
}
