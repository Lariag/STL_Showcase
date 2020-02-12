using HelixToolkit.Wpf;
using STL_Showcase.Logic.Files;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Extensions;
using STL_Showcase.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace STL_Showcase.Presentation.UI.Clases
{
    /// <summary>
    /// Handles the 3d view objects and configurations.
    /// </summary>
    public class View3D
    {

        public HelixViewport3D Viewport { get; private set; }
        ModelFileData ModelData;
        ModelVisual3D CurrentModelVisual;
        ModelVisual3D CurrentLightsVisual;
        RotateTransform3D transformObjectRotation;
        AxisAngleRotation3D CurrentAxisRotation;

        bool ModelAutoRotationEnabled;

        Point3D _originalCameraPosition;
        Point3D _modelCameraPosition;

        public RenderAspectEnum OptionRenderStyle { get; set; }

        public Action<string, int, int, int> LoadModelInfoAvailableEvent;

        public View3D(HelixViewport3D viewport)
        {
            this.Viewport = viewport;

            _originalCameraPosition = new Point3D(100f, -100f, 135f);
            _modelCameraPosition = _originalCameraPosition;

            viewport.Camera.UpDirection = new Vector3D(0, 0, 1);
            viewport.Camera.Position = _originalCameraPosition;
            viewport.Camera.LookDirection = new Vector3D(-1, 1, -1);

            Viewport.ShowCameraInfo = false;
            Viewport.ShowViewCube = false;
            Viewport.ShowCameraTarget = false;
            Viewport.ZoomAroundMouseDownPoint = false;
            Viewport.RotateAroundMouseDownPoint = false;
            Viewport.SnapMouseDownPoint = true;
            Viewport.FixedRotationPointEnabled = true;
            Viewport.FixedRotationPoint = new Point3D(0f, 0f, 30f);
            Viewport.IsZoomEnabled = false;


            Viewport.IsMoveEnabled = false;
            Viewport.IsPanEnabled = false;
            Viewport.IsManipulationEnabled = false;
            Viewport.IsHeadLightEnabled = false;

            Viewport.RotationSensitivity = 1;
            Viewport.ClipToBounds = true;

            ModelAutoRotationEnabled = true;

            GridLinesVisual3D FloorGridLines;
            FloorGridLines = new GridLinesVisual3D();
            FloorGridLines.Fill = Brushes.LightGray;
            FloorGridLines.Width = 15000;
            FloorGridLines.Thickness = 1;
            FloorGridLines.Length = FloorGridLines.Width;
            FloorGridLines.MajorDistance = 100;
            FloorGridLines.MinorDistance = 10;
            FloorGridLines.Center = new Point3D(0, 0, 0);

            UpdateLights();
            Viewport.Children.Add(FloorGridLines);

        }

        private IEnumerable<Point3D> Point3DFromLinearCoordinates(SegmentedArray<Half> vertices)
        {
            for (int i = 0; i + 2 < vertices.Length; i += 3)
                yield return new Point3D(vertices[i], vertices[i + 1], vertices[i + 2]);
        }
        private IEnumerable<Point3D> Point3DFromLinearCoordinates(SegmentedArray<STL_Showcase.Logic.Rendering.Mesh3D.Vertexh> vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
                yield return new Point3D(vertices[i].x, vertices[i].y, vertices[i].z);
        }

        private static Material GetMaterial()//(RenderAspectEnum renderAspect)
        {
            //if (renderAspect == RenderAspectEnum.AllGreen)
            //    return new DiffuseMaterial(new SolidColorBrush(Colors.LimeGreen));
            //else
            return new DiffuseMaterial(new SolidColorBrush(Colors.White));
        }
        private static IEnumerable<Light> GetLights(RenderAspectEnum renderAspect)
        {
            List<Light> lights = new List<Light>();
            if (renderAspect == RenderAspectEnum.VioletBlue)
            {
                lights.Add(new DirectionalLight(Colors.Violet, new Vector3D(-0.5, 1, -0.5)));
                lights.Add(new DirectionalLight(Colors.Blue, new Vector3D(-1, 0.5, -0.5)));
            }
            else if (renderAspect == RenderAspectEnum.RedOrangeYellow)
            {
                lights.Add(new DirectionalLight(Colors.Red, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.Yellow, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.Orange, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderAspect == RenderAspectEnum.GreenLimeYellow)
            {
                lights.Add(new DirectionalLight(Colors.Green, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.Yellow, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.LimeGreen, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderAspect == RenderAspectEnum.PinkFucsiaViolet)
            {
                lights.Add(new DirectionalLight(Colors.Pink, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.Fuchsia, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.Violet, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderAspect == RenderAspectEnum.CyanBlue)
            {
                lights.Add(new DirectionalLight(Colors.Blue, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.AliceBlue, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.Cyan, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderAspect == RenderAspectEnum.RedRedish)
            {
                lights.Add(new DirectionalLight(Colors.Red, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.DarkRed, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.PaleVioletRed, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderAspect == RenderAspectEnum.Yellow)
            {
                lights.Add(new DirectionalLight(Colors.Yellow, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.YellowGreen, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.LightYellow, new Vector3D(0.3, -0.3, -1)));
            }
            else
            {
                if (renderAspect == RenderAspectEnum.PerNormal)
                {
                    lights.Add(new DirectionalLight(Colors.DarkGreen, new Vector3D(-1, 0, 0)));
                    lights.Add(new DirectionalLight(Colors.DarkRed, new Vector3D(0, 1, 0)));
                    lights.Add(new DirectionalLight(Colors.DarkBlue, new Vector3D(0, 0, -1)));
                }

                lights.Add(new DirectionalLight(Colors.White, new Vector3D(-1, 1, -1)));
            }

            return lights;
        }
        public void SetModel(ModelFileData modelData)
        {

            if (CurrentModelVisual != null)
                this.Viewport.Children.Remove(CurrentModelVisual);
            CurrentModelVisual = null;

            Model3DGroup modelGroup = new Model3DGroup();
            ModelVisual3D modelVisual = new ModelVisual3D();

            try
            {
                if (modelData != null)
                {
                    ModelFileData newModelData = new ModelFileData(modelData.FileFullPath);
                    this.ModelData = newModelData;

                    newModelData.LoadBasicFileData();
                    if (!newModelData.HasBytes())
                        newModelData.LoadFileBytes(newModelData.FileSizeMB < 100f);
                    if (newModelData.Mesh == null)
                        newModelData.ParseFile();
                    newModelData.ReleaseData(true, false);

                    if (newModelData.Mesh == null)
                    {
                        newModelData.ReleaseData(true, false);
                        SetModel(null);
                        return;
                    }

                    LoadModelInfoAvailableEvent?.Invoke(newModelData.FileName, newModelData.Mesh.TriangleCount, newModelData.Mesh.Vertices.Length, (int)newModelData.FileSizeKB);

                    float modelScale = newModelData.Mesh.Scale / 4f;
                    float modelScaleMultiply = (newModelData.Mesh.Scale < 0.001f ? 0.1f : (newModelData.Mesh.Scale > 0.1 ? 10f : 1));


                    //{ // This loading is slower and eats lots of memory.
                    //    StLReader r = new HelixToolkit.Wpf.StLReader();
                    //    ModelImporter importet = new ModelImporter();

                    //    modelGroup = importet.Load(modelData.FileFullPath);
                    //}


                    var transformTranslate = new TranslateTransform3D(-newModelData.Mesh.OffsetX, -newModelData.Mesh.OffsetY, -newModelData.Mesh.OffsetZ);
                    AxisAngleRotation3D axisRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);
                    transformObjectRotation = new RotateTransform3D(axisRotation, new Point3D(newModelData.Mesh.OffsetX, newModelData.Mesh.OffsetY, 0));
                    ScaleTransform3D transformScale = new ScaleTransform3D(modelScaleMultiply, modelScaleMultiply, modelScaleMultiply);

                    Transform3DGroup transforms = new Transform3DGroup();
                    transforms.Children.Add(transformObjectRotation);
                    transforms.Children.Add(transformTranslate);
                    transforms.Children.Add(transformScale);

                    Mesh3D mesh = new Mesh3D(Point3DFromLinearCoordinates(newModelData.Mesh.Vertices), newModelData.Mesh.Triangles);
                    GeometryModel3D geometryModel = new GeometryModel3D(mesh.ToMeshGeometry3D(), GetMaterial());
                    geometryModel.Freeze();

                    modelGroup.Children.Add(geometryModel);
                    modelGroup.Freeze();

                    newModelData.ReleaseData(true, true);

                    // Animation 
                    if (ModelAutoRotationEnabled)
                    {
                        DoubleAnimation animation1 = new DoubleAnimation(-90, 395d, TimeSpan.FromMilliseconds(1000));
                        animation1.EasingFunction = new ExponentialEase();

                        DoubleAnimation animation2 = new DoubleAnimation(36d, 395d, TimeSpan.FromMilliseconds(7000));
                        animation2.RepeatBehavior = RepeatBehavior.Forever;

                        animation1.Completed += (o, e) => axisRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation2);
                        axisRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation1);
                    }

                    // Camera animation
                    var normalizedPosition = Viewport.Camera.Position.ToVector3D();
                    normalizedPosition.Normalize();
                    var nomalizedOriginalPosition = _originalCameraPosition.ToVector3D();
                    nomalizedOriginalPosition.Normalize();
                    _modelCameraPosition = new Point3D(nomalizedOriginalPosition.X / modelScale * modelScaleMultiply, nomalizedOriginalPosition.Y / modelScale * modelScaleMultiply, nomalizedOriginalPosition.Z / modelScale * modelScaleMultiply);

                    Viewport.Camera.AnimateTo(new Point3D(normalizedPosition.X / modelScale * modelScaleMultiply, normalizedPosition.Y / modelScale * modelScaleMultiply, normalizedPosition.Z / modelScale * modelScaleMultiply)
                    , Viewport.Camera.LookDirection, Viewport.Camera.UpDirection, 500d);

                    this.CurrentAxisRotation = axisRotation;
                    modelVisual.Transform = transforms;
                }
                modelVisual.Content = modelGroup;
                this.Viewport.Children.Add(modelVisual);

                this.CurrentModelVisual = modelVisual;

            }
            catch (Exception ex)
            {
                if (CurrentModelVisual != null)
                {
                    this.Viewport.Children.Add(CurrentModelVisual);
                    this.CurrentModelVisual = null;
                }
            }
            if (modelData == null)
            {
                _modelCameraPosition = _originalCameraPosition;
                ResetCamera();
                LoadModelInfoAvailableEvent?.Invoke("", 0, 0, 0);
            }
            GC.Collect();
        }


        public void UpdateLights()
        {
            // Remove current lights.
            if (CurrentLightsVisual != null)
                this.Viewport.Children.Remove(CurrentLightsVisual);
            CurrentLightsVisual = null;

            // Set new lights based on the render style.
            Model3DGroup modelGroup = new Model3DGroup();
            ModelVisual3D modelVisual = new ModelVisual3D();
            foreach (var light in GetLights(OptionRenderStyle))
                modelGroup.Children.Add(light);

            // Add lights.
            modelVisual.Content = modelGroup;
            this.Viewport.Children.Add(modelVisual);
            CurrentLightsVisual = modelVisual;
        }

        public void ResetCamera()
        {
            Viewport.Camera.AnimateTo(_modelCameraPosition, new Vector3D(-1, 1, -1), new Vector3D(0, 0, 1), 500d);
        }
        public void SetCameraRotationMode(bool enableAutoRotation)
        {
            ModelAutoRotationEnabled = enableAutoRotation;

            if (CurrentModelVisual == null || CurrentAxisRotation == null)
                return;

            if (ModelAutoRotationEnabled)
            {
                DoubleAnimation animation1 = new DoubleAnimation(CurrentAxisRotation.Angle, CurrentAxisRotation.Angle + 420d, TimeSpan.FromMilliseconds(500));
                animation1.EasingFunction = new ExponentialEase();

                DoubleAnimation animation2 = new DoubleAnimation(CurrentAxisRotation.Angle + 420d, CurrentAxisRotation.Angle + 420d + 360d, TimeSpan.FromMilliseconds(7000));
                animation2.RepeatBehavior = RepeatBehavior.Forever;

                animation1.Completed += (o, e) => CurrentAxisRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation2);
                CurrentAxisRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation1);
            }
            else
            {
                DoubleAnimation animation1 = new DoubleAnimation(CurrentAxisRotation.Angle % 360f, 0, TimeSpan.FromMilliseconds(500));
                animation1.EasingFunction = new ExponentialEase();
                CurrentAxisRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation1);
            }
        }
        public BitmapSource TakeSnapshot()
        {
            return null;
        }
        public void UnloadModel()
        {
            this.SetModel(null);
        }
        //if (modelData != null)
        //{
        //    StLReader r = new HelixToolkit.Wpf.StLReader();
        //    ModelImporter importet = new ModelImporter();

        //    //Model3DGroup model3dgroup = r.Read( modelData.FileFullPath );
        //    Model3DGroup modelGroup = importet.Load(modelData.FileFullPath);

        //    ModelVisual3D loadedModelVisual = new ModelVisual3D();

        //    GeometryModel3D loadedModel = modelGroup.Children.FirstOrDefault() as GeometryModel3D;
        //    if (loadedModel != null)
        //    {
        //        loadedModelVisual.Content = loadedModel;
        //    }
        //    else
        //    {
        //        // Set the error file.
        //    }

        //    this.Viewport.Children.Add(loadedModelVisual);


        //    CurrentModelVisual = loadedModelVisual;
        //    CurrentModel = loadedModel;
        //}


    }
}
