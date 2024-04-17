using Model3D.Tools.Vectorization;
using System;

namespace ViewMotion;

partial class SceneMotion
{
    private readonly Vectorizer vectorizer;
    private readonly Random rnd;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
        this.rnd = new Random(0);
    }
}
