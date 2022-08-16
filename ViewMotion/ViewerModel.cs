using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using ViewMotion.Annotations;
using ViewMotion.Extensions;
using ViewMotion.Models;
using LightType = ViewMotion.Models.LightType;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Shape = Model.Shape;

namespace ViewMotion
{
    partial class ViewerModel : INotifyPropertyChanged
    {
        
        //public ICommand AddCommand
        //{
        //    get
        //    {
        //        return addCommand ??
        //               (addCommand = new (obj =>
        //               {
        //                   Phone phone = new Phone();
        //                   Phones.Insert(0, phone);
        //                   SelectedPhone = phone;
        //               }));
        //    }
        //}

        public bool IsControlPanelVisible
        {
            get => isControlPanelVisible;
            set
            {
                isControlPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public PerspectiveCamera Camera { get; }
        public ModelVisual3D Model { get; set; } = new();
        public List<ModelVisual3D> Lights { get; } = new();

        public Action<ModelVisual3D> UpdateModel { get; set; }

        private Settings settings;
        private readonly SceneMotion scene;

        private Dictionary<Model.Material, Material> materials = new();
        private bool isControlPanelVisible = true;

        public Material GetMaterial(Model.Material? m)
        {
            var color = m?.Color.ToWColor() ?? Colors.Black;
            m ??= default!;

            if (materials.TryGetValue(m, out Material? material))
                return material;

            material = new MaterialGroup()
            {
                Children =
                {
                    new DiffuseMaterial
                    {
                        Brush = new SolidColorBrush(color)
                    },
                    new SpecularMaterial()
                    {
                        Brush = new SolidColorBrush(color) {Opacity = 0.5},
                        SpecularPower = 24
                    },
                }
            };

            materials.Add(m, material);

            return material;
        }

        public void RefreshCamera()
        {
            Camera.Position = settings.CameraOptions.Position.ToP3D();
            Camera.LookDirection = settings.CameraOptions.LookDirection.ToV3D();
        }

        private List<Shape> frameShapes = new();

        private void RefreshShape(Shape sceneShape)
        {
            if (settings.AllowFrameHistory)
                frameShapes.Add(sceneShape);

            var model = new ModelVisual3D();

            foreach (var shape in sceneShape.SplitByMaterial())
            {
                var visual = new ModelVisual3D()
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
                        GetMaterial(shape.Materials?[0])
                    )
                };

                model.Children.Add(visual);
            }

            Model = model;
            UpdateModel?.Invoke(model);
        }

        async Task Play(Motion motion)
        {
            var count = 0;
            while (true)
            {
                await Task.WhenAll(motion.Step(++count, RefreshShape), Task.Delay(10));
            }
        }

        public ViewerModel(Settings settings, SceneMotion scene)
        {
            this.settings = settings;
            this.scene = scene;

            Camera = new PerspectiveCamera(
                settings.CameraOptions.Position.ToP3D(), 
                settings.CameraOptions.LookDirection.ToV3D(), 
                settings.CameraOptions.UpDirection.ToV3D(), 
                settings.CameraOptions.FieldOfView);

            foreach (var lightOptions in settings.Lights)
            {
                Lights.Add(new ModelVisual3D()
                {
                    Content = lightOptions.LightType switch
                    {
                        LightType.Directional => new DirectionalLight(lightOptions.Color, lightOptions.Direction.ToV3D()),
                        LightType.Ambient => new AmbientLight(lightOptions.Color),
                        _ => throw new ArgumentOutOfRangeException()
                    }
                });
            }

            var motion = scene.Scene().GetAwaiter().GetResult();

            RefreshShape(motion.Shape);

            Play(motion);
        }

        private IEnumerable<Point> ToDefaultTexturePoints(int[] convex)
        {
            Point GetPoint(double alfa) => new Point(Math.Cos(alfa), Math.Sign(alfa));
            var ps = (convex.Length).SelectRange(i => GetPoint(2 * Math.PI * (i + 0.5) / convex.Length)).ToArray();
            var schema = Shape.TriangleSchemaList(convex.Length);

            return schema.Select(i => ps[i]);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
