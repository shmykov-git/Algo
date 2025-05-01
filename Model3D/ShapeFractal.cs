using Model3D.AsposeModel;
using Model;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model3D
{
    public class ShapeTreeFractal
    {
        public Step[] Steps;

        private Shape ApplyRule(Shape shape, Rule r) => shape.Transform(p => r.Apply(p));

        private Rule JoinRules(Rule a, Rule b) => new Rule
        {
            Point = a.Apply(b.Point),
            Direction = (a.Direction + b.Direction).Normalize(),
            Scale = a.Scale * b.Scale
        };

        public Shape CreateFractal(int count)
        {
            List<Shape> fractal = new List<Shape>();

            var levelRules = new[] { BaseRule };
            for (var i = 0; i < count; i++)
            {
                var step = Steps[i % Steps.Length];
                var levelShapes = levelRules.Select(r => ApplyRule(step.Shape, r)).ToArray();
                levelRules = levelRules.SelectMany(a => step.Rules.Select(b => JoinRules(a, b))).ToArray();
                fractal.AddRange(levelShapes);
            }

            return fractal.Aggregate((a, b) => a + b);
        }

        public static Rule BaseRule => new Rule
        { 
            Point = new Vector3(0, 0, 0),
            Direction = new Vector3(0, 0, 1),
            Scale = 1
        };

        public class Step
        {
            public Shape Shape;
            public Rule[] Rules;
        }

        public class Rule
        {
            public Vector3 Point;
            public Vector3 Direction;
            public double Scale;

            private Quaternion q => Quaternion.FromRotation(Vector3.ZAxis, Direction);

            public Vector3 Apply(Vector3 p) => Point + q * p * Scale;
        }
    }

}
