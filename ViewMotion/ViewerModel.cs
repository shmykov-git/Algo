using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using ViewMotion;
using ViewMotion.Annotations;
using ViewMotion.Commands;
using ViewMotion.Extensions;
using ViewMotion.Models;
using LightType = ViewMotion.Models.LightType;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Shape = Model.Shape;

namespace ViewMotion
{
    partial class ViewerModel : INotifyPropertyChanged
    {
        private bool isCalculating = true;
        private bool isPlaying = false;
        private List<Action> buttonRefreshes = new();

        private void SaveRefresh(Action refresh) => buttonRefreshes.Add(refresh);
        private void RefreshButtons() => buttonRefreshes.ForEach(refresh => refresh());

        private void Refresh()
        {
            RefreshButtons();
            OnPropertyChanged(nameof(CalcName));
            OnPropertyChanged(nameof(ReplayName));
            OnPropertyChanged(nameof(CanCalc));
        }

        public double Speed { get; set; }
        public bool IsAutoReplay { get; set; } = true;

        public string ReplayName => isPlaying ? "■ Stop Playing" : "► Play";
        
        public ICommand ReplayCommand => new Command(() =>
        {
            if (isPlaying)
                isPlaying = false;
            else
                Play();
            Refresh();
        }, () => !isCalculating, SaveRefresh);

        public ICommand CalcCommand => new Command(() =>
        {
            isCalculating = !isCalculating;
            Refresh();
        }, () => !isPlaying, SaveRefresh);

        public string CalcName => isCalculating ? "■ Stop Calculation" : "► Calculate";
        public bool CanCalc { get; set; } = true;
        public bool IsControlPanelVisible
        {
            get => isControlPanelVisible;
            set
            {
                isControlPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public string FrameInfo
        {
            get => frameInfo;
            set
            {
                frameInfo = value;
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

        private Model.Material defaultMaterial = new() {Color = System.Drawing.Color.Black};
        public Material GetMaterial(Model.Material? m)
        {
            m ??= defaultMaterial;
            var color = m.Color.ToWColor();

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

        private List<ViewState> viewStates = new();
        private string frameInfo;

        private void OnNewCalculatedFrame(Shape frameShape)
        {
            var viewState = GetViewState(frameShape);

            if (settings.AllowFrameHistory)
                viewStates.Add(viewState);

            FrameInfo = $"Frames: {viewStates.Count}";
            ShowViewShape(viewState);
        }

        private ViewState GetViewState(Shape frameShape)
        {
            var viewShapes = frameShape.SplitByMaterial().Select(shape => new ViewShape()
            {
                Positions = new Point3DCollection(shape.Points3.Select(p => p.ToP3D())),
                TriangleIndices = new Int32Collection(shape.Triangles),
                Normals = new Vector3DCollection(shape.PointNormals.Select(p => p.ToV3D())),
                TextureCoordinates = shape.TexturePoints == null
                    ? new PointCollection(shape.Convexes.SelectMany(ToDefaultTexturePoints))
                    : new PointCollection(shape.TriangleTexturePoints.Select(p => p.ToP2D())),
                Material = GetMaterial(shape.Materials?[0])
            }).ToArray();

            return new ViewState()
            {
                ViewShapes = viewShapes
            };
        }

        private async Task ShowViewShape(ViewState state)
        {
            var model = new ModelVisual3D();

            foreach (var viewShape in state.ViewShapes)
            {
                var visual = new ModelVisual3D()
                {
                    Content = new GeometryModel3D(
                        new MeshGeometry3D()
                        {
                            Positions = viewShape.Positions,
                            TriangleIndices = viewShape.TriangleIndices,
                            Normals = viewShape.Normals,
                            TextureCoordinates = viewShape.TextureCoordinates
                        },
                        viewShape.Material
                    )
                };

                model.Children.Add(visual);
            }

            Model = model;
            UpdateModel?.Invoke(model);
        }

        async Task Play()
        {
            isPlaying = true;

            do
            {
                foreach (var shape in viewStates.ToArray())
                {
                    await Task.WhenAll(ShowViewShape(shape), Task.Delay((int)(5 + 20 * Speed)));

                    if (!isPlaying)
                        break;
                }
            } while (IsAutoReplay && isPlaying);

            isPlaying = false;
            Refresh();
        }

        async Task CalculateFrames(Motion motion)
        {
            var count = 0;
            while (true)
            {
                if (isCalculating)
                    if (!await motion.Step(++count, OnNewCalculatedFrame))
                    {
                        isCalculating = false;
                        CanCalc = false;

                        break;
                    }
                else
                    await Task.Delay(10);
            }

            Refresh();
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

            if (motion.Shape != null)
                OnNewCalculatedFrame(motion.Shape);

            CalculateFrames(motion);
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

        class ViewState
        {
            public ViewShape[] ViewShapes;
        }

        class ViewShape
        {
            public Point3DCollection Positions;
            public Int32Collection TriangleIndices;
            public Vector3DCollection Normals;
            public PointCollection TextureCoordinates;
            public Material Material;
        }
    }
}
