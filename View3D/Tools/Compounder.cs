using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using View3D.Libraries;

namespace View3D.Tools
{

    class Compounder
    {

        public Scene Plane(PoligonInfo poligonInfo) => CreateScene(poligonInfo, PlaneShapes.Plane);
        public Scene Cube(PoligonInfo poligonInfo) => CreateScene(poligonInfo, PlaneShapes.Cube);


        public Scene CreateScene(PoligonInfo poligonInfo, PlaneShape planeShape)
        {
            Scene scene = new Scene();
            Node node = scene.RootNode.CreateChildNode("main");

            node.Entity = CreateMesh(poligonInfo, planeShape);

            return scene;
        }

        private Mesh CreateMesh(PoligonInfo poligonInfo, PlaneShape planeShape)
        {
            Vector4[] basePoints = poligonInfo.Poligon.Points.Select(p => new Vector4(p.X, p.Y, 0, 1)).ToArray();

            Vector4[] controlPoints = planeShape.Transformations.SelectMany(ts => basePoints.Select(p =>
            {
                foreach (var f in ts)
                    p = f(p);
                return p;
            })).ToArray();

            var mesh = new Mesh();
            mesh.ControlPoints.AddRange(controlPoints);

            for (var k = 0; k < planeShape.Transformations.Length; k++)
                foreach (var convex in poligonInfo.Convexes)
                {
                    mesh.CreatePolygon(convex.Select(i => k * poligonInfo.Poligon.Points.Length + i).ToArray());
                }

            return mesh;
        }
    }
}
