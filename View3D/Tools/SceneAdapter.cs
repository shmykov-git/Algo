using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View3D.Tools
{
    class SceneAdapter
    {
        public Scene CreateScene(PoligonInfo poligonInfo)
        {
            Scene scene = new Scene();
            Node node = scene.RootNode.CreateChildNode("main");

            node.Entity = CreateMesh(poligonInfo);

            return scene;
        }

        private Mesh CreateMesh(PoligonInfo poligonInfo)
        {
            Vector4[] controlPoints = poligonInfo.Poligon.Points.Select(p => new Vector4(p.X, p.Y, 0, 1)).ToArray();

            var mesh = new Mesh();
            mesh.ControlPoints.AddRange(controlPoints);

            foreach (var trio in poligonInfo.Trios)
            {
                mesh.CreatePolygon(trio.ToArray());
            }

            return mesh;
        }
    }
}
