using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using ViewMotion.Extensions;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;

namespace ViewMotion
{
    partial class ViewerModel
    {
        public PerspectiveCamera Camera { get; }
        public List<ModelVisual3D> VisualElements { get; } = new();

        private Settings settings;
        private readonly MotionScene scene;

        public void RefreshCamera()
        {
            Camera.Position = settings.CameraOptions.Position.ToP3D();
            Camera.LookDirection = settings.CameraOptions.LookDirection.ToV3D();
        }

        public ViewerModel(Settings settings, MotionScene scene)
        {
            this.settings = settings;
            this.scene = scene;

            Camera = new PerspectiveCamera(
                settings.CameraOptions.Position.ToP3D(), 
                Vector3Extensions.ToV3D(settings.CameraOptions.LookDirection), 
                Vector3Extensions.ToV3D(settings.CameraOptions.UpDirection), 
                settings.CameraOptions.FieldOfView);

            foreach (var lightOptions in settings.Lights)
            {
                VisualElements.Add(new ModelVisual3D()
                {
                    Content = new DirectionalLight(lightOptions.Color, Vector3Extensions.ToV3D(lightOptions.Direction))
                });
            }

            foreach (var shape in scene.StaticScene().SelectMany(s=>s.SplitByMaterial()))
            {
                var color = shape.Materials?[0].Color.ToWColor() ?? Colors.Black;

                VisualElements.Add(new ModelVisual3D()
                {
                    Content = new GeometryModel3D(
                        new MeshGeometry3D()
                        {
                            Positions = new Point3DCollection(shape.Points3.Select(p => p.ToP3D())),
                            TriangleIndices = new Int32Collection(shape.Triangles),
                            Normals = new Vector3DCollection(shape.PointNormals.Select(p => p.ToV3D())),
                            TextureCoordinates = shape.TexturePoints == null
                                ? new PointCollection(shape.Convexes.SelectMany(ToDefaultTexturePoints))
                                : new PointCollection(shape.TriangleTexturePoints.Select(p => p.ToP2D()))
                        },
                        new DiffuseMaterial
                        {
                            Brush = new SolidColorBrush(color)
                        })
                });
            }
        }

        private Point[] defaultPoints3 = {new(0, 0), new(1, 0), new(1, 1)};
        private Point[] defaultPoints4 = {new(0, 0), new(1, 0), new(1, 1), new(0, 1)};
        private int[] ts3 = {0, 1, 2};
        private int[] ts4 = { 0, 1, 2, 2, 3, 0 };

        private IEnumerable<Point> ToDefaultTexturePoints(int[] convex)
        {
            return convex.Length switch
            {
                4 => ts4.Select(i => defaultPoints4[i]).ToArray(),
                3 => ts3.Select(i => defaultPoints3[i]).ToArray(),
                _ => ts3.Select(i => defaultPoints3[i]).ToArray(),
            };
        }

    }
}
