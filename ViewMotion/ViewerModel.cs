using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using ColorPicker;
using ColorPicker.Models;
using meta.Extensions;
using Meta.Extensions;
using Model.Extensions;
using Model3D.Extensions;
using View3D;
using View3D.Tools;
using ViewMotion;
using ViewMotion.Annotations;
using ViewMotion.Commands;
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using Light = System.Windows.Media.Media3D.Light;
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
        private ViewState lastViewState = null;
        private Color _bc;
        private ColorState? _bcs = null;
        private bool _cpv = false;
        private PersistState persistState;

        private List<ViewState> viewStates = new();
        private string frameInfo;
        private double light;

        private CameraMotionOptions? cameraMotionOptions;
        
        public ViewerModel(MotionSettings motionSettings, SceneMotion scene, StaticSceneRender staticRender, StaticSettings staticSettings)
        {
            this.motionSettings = motionSettings;
            this.staticRender = staticRender;
            this.staticSettings = staticSettings;

            persistState = GetPersistState();
            BackgroundColorState = persistState.BackgroundColorState;

            var motion = scene.Scene().Result;
            cameraMotionOptions = motion.CameraMotionOptions;
            
            if (cameraMotionOptions?.CameraStartOptions != null)
                motionSettings.CameraOptions = cameraMotionOptions.CameraStartOptions;

            Camera = new PerspectiveCamera(
                motionSettings.CameraOptions.Position.ToP3D(),
                motionSettings.CameraOptions.LookDirection.ToV3D(),
                motionSettings.CameraOptions.UpDirection.ToV3D(),
                motionSettings.CameraOptions.FieldOfView);

            foreach (var lightOptions in motionSettings.Lights)
            {
                Lights.Add(new ModelVisual3D()
                {
                    Content = GetLight(lightOptions)
                });
            }

            if (motion.Shape != null)
                OnNewCalculatedFrame(motion.Shape);

            if (motion.CameraDistance.HasValue)
            {
                motionSettings.CameraOptions.Position = motionSettings.CameraOptions.Position.ToLen(motion.CameraDistance.Value);
                motionSettings.CameraOptions.LookDirection = -motionSettings.CameraOptions.Position.Normalize();
                RefreshCamera();
            }

            CalculateFrames(motion);
        }

        public string[] Animations => new[] {"No animation", "Fly around", "Fly far around", "Fly near around", "Fly around before"};

        public int AnimationIndex { get; set; }

        private void SetCameraAnimation()
        {
            cameraMotionOptions = AnimationIndex switch
            {
                0 => null,
                1 => CameraAnimations.FlyAround(motionSettings.CameraOptions.Position, motionSettings.CameraOptions.LookDirection),
                2 => CameraAnimations.FlyAround(motionSettings.CameraOptions.Position, motionSettings.CameraOptions.LookDirection, 0.5),
                3 => CameraAnimations.FlyAround(motionSettings.CameraOptions.Position, motionSettings.CameraOptions.LookDirection, -0.25),
                4 => CameraAnimations.FlyAround(motionSettings.CameraOptions.Position, motionSettings.CameraOptions.LookDirection, -0.7),
                _ => throw new NotImplementedException()
            };
        }

        private void SaveRefresh(Action refresh) => buttonRefreshes.Add(refresh);
        private void RefreshButtons() => buttonRefreshes.ForEach(refresh => refresh());

        private void Refresh()
        {
            RefreshCamera();
            RefreshButtons();
            OnPropertyChanged(nameof(CalcName));
            OnPropertyChanged(nameof(ReplayName));
            OnPropertyChanged(nameof(CanCalc));
        }

        public double Speed { get; set; }
        public int FrameNumber { get => frameNumber; set { frameNumber = value; OnPropertyChanged(); RefreshView(); } }
        public int FrameMaxNumber { get => frameMaxNumber; set { frameMaxNumber = value; OnPropertyChanged(); } }

        public double Light
        {
            get => light;
            set
            {
                light = value;
                OnPropertyChanged(nameof(Light));
                RefreshLights();
            }
        }

        private void RefreshView()
        {
            if (isPlaying | isCalculating)
                return;

            ShowFrame(FrameNumber, viewStates.Count);
            ShowViewShape(viewStates[FrameNumber]);
        }

        private void RefreshLights()
        {
            var alfa = 2 * Math.PI * Light * 0.1;
            var q = Quaternion.FromEulerAngle(alfa, 0, 0);
            motionSettings.Lights.ForEach((l, i) =>
            {
                if (l.LightType == LightType.Ambient)
                    return;

                var newL = l.With(ll=>ll.Direction *= q);
                Lights[i].Content = GetLight(newL);
            });
        }

        public bool IsAutoReplay { get; set; } = true;
        public bool IsReverseReplay { get; set; } = false;

        public string ExportName => "⇒ Export";
        public ICommand ExportCommand => new Command(() =>
        {
            var frameShape = GetShapeFromViewState(lastViewState);
            var staticScene = staticRender.CreateScene(frameShape);
            staticScene.Save(staticSettings.FullFileName, staticSettings.Format);
            ShowStaticScene(staticSettings.FullFileName);
        }, () => lastViewState != null, SaveRefresh);

        private void ShowStaticScene(string fullFileName)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = fullFileName;
            process.Start();
        }

        public string ReplayName => isPlaying ? "■ Stop Playing" : "► Play";

        public ICommand ReplayCommand => new Command(DoReplay, () => !isCalculating, SaveRefresh);

        public Brush BackgroundColorBrush { get => new SolidColorBrush(BackgroundColor); }
        public ColorState BackgroundColorState 
        { 
            get => _bcs??_bc.ToState(); 
            set 
            { 
                _bcs = value;
                persistState.BackgroundColorState = value;
                SavePersistState(persistState);
                BackgroundColor = value.FromState(); 
            } 
        }

        public Color BackgroundColor 
        { 
            get => _bc; set 
            { 
                _bc = value;
                OnPropertyChanged(nameof(BackgroundColorBrush)); 
            } 
        }

        public Brush[] SavedColorBrushes => persistState.SavedColorStates.Select(c => new SolidColorBrush(c.FromState())).ToArray();

        public string ChangeBcName => "Background color";

        public ICommand ChangeBcCommand => new Command(() => 
        { 
            IsColorPickerVisible = !IsColorPickerVisible; 
        }, () => !isCalculating, SaveRefresh);

        public bool IsColorPickerVisible { get => _cpv; set { _cpv = value; OnPropertyChanged(); } }

        private void DoReplay()
        {
            if (isPlaying)
                isPlaying = false;
            else
                Play();
            Refresh();
        }

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

        private MotionSettings motionSettings;
        private readonly StaticSceneRender staticRender;
        private readonly View3D.StaticSettings staticSettings;

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
                    }
                }
            };

            materials.Add(m, material);

            return material;
        }

        public void RefreshCamera()
        {
            if (cameraMotionOptions != null && (isPlaying || isCalculating))
            {
                var step = isCalculating ? viewStates.Count : frameNum;

                if (cameraMotionOptions.CameraFn != null)
                {
                    var c = motionSettings.CameraOptions;

                    (c.Position, c.LookDirection, c.UpDirection) = cameraMotionOptions.CameraFn(step, viewStates.Count);
                }
            }

            Camera.Position = motionSettings.CameraOptions.Position.ToP3D();
            Camera.LookDirection = motionSettings.CameraOptions.LookDirection.ToV3D();
            Camera.UpDirection = motionSettings.CameraOptions.UpDirection.ToV3D();
        }

        private void OnNewCalculatedFrame(Shape frameShape)
        {
            var viewState = GetViewState(frameShape);

            if (motionSettings.AllowFrameHistory)
                viewStates.Add(viewState);

            FrameMaxNumber = viewStates.Count - 1;
            FrameNumber = viewStates.Count - 1;

            ShowFrame(FrameNumber, viewStates.Count);
            RefreshButtons();
            RefreshCamera();
            ShowViewShape(viewState);
        }

        private ViewState GetViewState(Shape frameShape)
        {
            var viewShapes = frameShape.SplitByMaterial().Select(shape => new ViewShape()
            {
                Positions = new Point3DCollection(shape.Points3.Select(p => p.ToP3D())),
                TriangleIndices = new Int32Collection(shape.Triangles),
                //Normals = new Vector3DCollection(shape.PointNormals.Select(p => p.ToV3D())),
                TextureCoordinates = shape.TexturePoints == null
                    ? new PointCollection(shape.Convexes.SelectMany(ToDefaultTexturePoints))
                    : new PointCollection(shape.TriangleTexturePoints.Select(p => p.ToP2D())),
                ModelMaterial = shape.Materials?[0],
                Material = GetMaterial(shape.Materials?[0])
            }).ToArray();

            return new ViewState()
            {
                ViewShapes = viewShapes
            };
        }

        private Shape GetShapeFromViewState(ViewState viewState)
        {
            return viewState.ViewShapes.Select(s =>
            {
                var ss= new Shape()
                {
                    Points3 = s.Positions.Select(p => new Vector3(p.X, p.Y, p.Z)).ToArray(),
                    Convexes = s.TriangleIndices.SelectByTriple().Select(t => new[] {t.a, t.b, t.c}).ToArray(),
                    Materials = (s.TriangleIndices.Count / 3).SelectRange(_ => s.ModelMaterial)
                        .ToArray(), //todo: check null
                    TexturePoints = null, //todo: not supported
                };

                return ss;
            }).ToSingleShape();
        }

        private void ShowFrame(int frameNumber, int framesCount)
        {
            FrameInfo = $"Frame: {frameNumber + 1} from {framesCount}";
        }

        private async Task ShowViewShape(ViewState state)
        {
            lastViewState = state;
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
                            //Normals = viewShape.Normals,
                            TextureCoordinates = viewShape.TextureCoordinates,
                        },
                        viewShape.Material
                    )
                };

                model.Children.Add(visual);
            }

            Model = model;
            UpdateModel?.Invoke(model);
        }

        private int frameNum = 0;
        private int frameNumber;
        private int frameMaxNumber;

        async Task Play()
        {
            isPlaying = true;

            SetCameraAnimation();
            ViewState? shape0 = null;

            do
            {
                IEnumerable<ViewState> states = viewStates;
                
                if (IsReverseReplay)
                    states = states.Concat(states.Reverse());

                foreach (var (shape, i) in states.Select((s,i)=>(s,i)).ToArray())
                {
                    frameNum = i < viewStates.Count ? i : 2 * viewStates.Count - i - 1;
                    ShowFrame(frameNum, viewStates.Count);
                    FrameNumber = frameNum;

                    await Task.WhenAll(ShowViewShape(shape), Task.Delay((int)(5 + 20 * Speed)));
                    RefreshCamera();

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
            while (CanCalc)
            {
                if (isCalculating)
                {
                    if (!await motion.Step(++count, OnNewCalculatedFrame))
                    {
                        isCalculating = false;
                        CanCalc = false;
                    }   
                }
                else
                {
                    await Task.Delay(10);
                }
            }

            Refresh();
        }

        Light GetLight(LightOptions lightOptions)
        {
            return lightOptions.LightType switch
            {
                LightType.Directional => new DirectionalLight(lightOptions.Color, lightOptions.Direction.ToV3D()),
                LightType.Ambient => new AmbientLight(lightOptions.Color),
                _ => throw new ArgumentOutOfRangeException()
            };
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
            //public Vector3DCollection Normals;
            public PointCollection TextureCoordinates;
            public Material Material;
            public Model.Material ModelMaterial;
        }

        private string persistFileName = "persist.json";
        PersistState GetPersistState()
        {
            var str = File.Exists(persistFileName) ? File.ReadAllText("persist.json") : null;

            return str?.FromJson<PersistState>() ?? new PersistState() 
            { 
                BackgroundColorState = Colors.White.ToState(),
                SavedColorStates = new[]
                {
                    Colors.White.ToState(),
                    Colors.White.ToState(),
                    Colors.White.ToState(),
                    Colors.White.ToState(),
                    Colors.White.ToState(),
                }
            };
        }
        
        private void SavePersistState(PersistState persistState)
        {
            File.WriteAllText(persistFileName, persistState.ToJson());
        }

        class PersistState
        {
            public required ColorState BackgroundColorState { get; set; }
            public required ColorState[] SavedColorStates { get; set; }
        }
    }
}
