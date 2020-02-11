using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using HelixToolkit;
using HelixToolkit.Wpf;

namespace STL_Showcase.Logic.Rendering
{
    class RenderEnv_ViewPort3D : IRenderEnviorement
    {

        Viewport3D viewport;
        Model3DGroup myModel3DGroup;
        ModelVisual3D myModelVisual3D;
        PerspectiveCamera myPCamera;
        GeometryModel3D myGeometryModel;
        GridLinesVisual3D myGridLines;
        RenderAspectEnum renderType;
        int sizeInPixels;

        public RenderEnv_ViewPort3D(int sizeInPixels)
        {
            this.sizeInPixels = sizeInPixels;
        }
        public void SetEnviorementOptions(RenderAspectEnum renderType)
        {
            this.renderType = renderType;

            viewport = new Viewport3D();

            myModel3DGroup = new Model3DGroup();
            myModelVisual3D = new ModelVisual3D();
            myPCamera = new PerspectiveCamera();
            RenderOptions.SetEdgeMode(viewport, EdgeMode.Unspecified);

            // Setup camera.
            myPCamera.UpDirection = new Vector3D(0, 0, 1);
            myPCamera.Position = new Point3D(2, -2, 2.75f);
            myPCamera.LookDirection = new Vector3D(-1, 1, -1);
            myPCamera.FieldOfView = 40;
            viewport.Camera = myPCamera;

            // Lights
            List<Light> lights = new List<Light>();
            if (renderType == RenderAspectEnum.VioletBlue)
            {
                lights.Add(new DirectionalLight(Colors.Violet, new Vector3D(-0.5, 1, -0.5)));
                lights.Add(new DirectionalLight(Colors.Blue, new Vector3D(-1, 0.5, -0.5)));
            }
            else if (renderType == RenderAspectEnum.RedOrangeYellow)
            {
                lights.Add(new DirectionalLight(Colors.Red, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.Yellow, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.Orange, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderType == RenderAspectEnum.GreenLimeYellow)
            {
                lights.Add(new DirectionalLight(Colors.Green, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.Yellow, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.LimeGreen, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderType == RenderAspectEnum.PinkFucsiaViolet)
            {
                lights.Add(new DirectionalLight(Colors.Pink, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.Fuchsia, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.Violet, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderType == RenderAspectEnum.CyanBlue)
            {
                lights.Add(new DirectionalLight(Colors.Blue, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.AliceBlue, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.Cyan, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderType == RenderAspectEnum.RedRedish)
            {
                lights.Add(new DirectionalLight(Colors.Red, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.DarkRed, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.PaleVioletRed, new Vector3D(0.3, -0.3, -1)));
            }
            else if (renderType == RenderAspectEnum.Yellow)
            {
                lights.Add(new DirectionalLight(Colors.Yellow, new Vector3D(-1, -0.3, 0)));
                lights.Add(new DirectionalLight(Colors.YellowGreen, new Vector3D(0.3, 1, 0)));
                lights.Add(new DirectionalLight(Colors.LightYellow, new Vector3D(0.3, -0.3, -1)));
            }
            else
            {
                if (renderType == RenderAspectEnum.PerNormal)
                {
                    lights.Add(new DirectionalLight(Colors.DarkGreen, new Vector3D(-1, 0, 0)));
                    lights.Add(new DirectionalLight(Colors.DarkRed, new Vector3D(0, 1, 0)));
                    lights.Add(new DirectionalLight(Colors.DarkBlue, new Vector3D(0, 0, -1)));
                }

                lights.Add(new DirectionalLight(Colors.White, new Vector3D(-1, 1, -1)));
            }

            foreach (var light in lights)
                myModel3DGroup.Children.Add(light);

            // Add a grid
            myGridLines = new GridLinesVisual3D();
            myGridLines.Fill = Brushes.LightGray;

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;
            viewport.Children.Add(myModelVisual3D);

        }
        public void SetModel(Mesh3D mesh)
        {
            // Add the mesh.
            myGeometryModel = new GeometryModel3D();
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();
            Point3DCollection myPositionCollection = new Point3DCollection();
            Int32Collection myTriangleIndicesCollection = new Int32Collection();

            try
            {
                for (int i = 0; i < mesh.Vertices.Length; i += 3)
                {
                    myPositionCollection.Add(new Point3D(mesh.Vertices[i], mesh.Vertices[i + 1], mesh.Vertices[i + 2]));
                }
                myTriangleIndicesCollection = new Int32Collection(mesh.Triangles);
            }
            catch (Exception ex)
            {

            }

            myMeshGeometry3D.Positions = myPositionCollection;
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            // Define material and apply to the mesh geometries.
            Material myMaterial;
            //if (this.renderType == RenderAspectEnum.GreenLimeYellow)
            //    myMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LimeGreen));
            //else
            myMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.White));

            myGeometryModel.Material = myMaterial;

            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new ScaleTransform3D(mesh.Scale, mesh.Scale, mesh.Scale));
            transformGroup.Children.Add(new TranslateTransform3D(new Vector3D(-mesh.OffsetX * mesh.Scale, -mesh.OffsetY * mesh.Scale, -mesh.OffsetZ * mesh.Scale)));
            myModel3DGroup.Transform = transformGroup;

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            float newScale = mesh.Scale < 0.001 ? mesh.Scale * 10 : (mesh.Scale > 0.1 ? mesh.Scale * 0.1f : mesh.Scale);
            myGridLines.Width = 5000 * newScale;
            myGridLines.Thickness = myGridLines.Width / 7000;
            myGridLines.Length = myGridLines.Width;
            myGridLines.MajorDistance = myGridLines.Width / 30;
            myGridLines.MinorDistance = myGridLines.MajorDistance / 10;
            myGridLines.Center = new Point3D(-3f * mesh.Scale + myGridLines.MajorDistance / 2f, 3f * mesh.Scale + myGridLines.MajorDistance / 2f, -27.5f * mesh.Scale);
        }
        public void RemoveModel()
        {
            if (myGeometryModel == null) return;

            myModel3DGroup.Children.Remove(myGeometryModel);
            myGeometryModel = null;
        }
        public BitmapSource RenderImage()
        {
            try
            {
                if (sizeInPixels > 48)
                    viewport.Children.Add(myGridLines);

                viewport.Width = sizeInPixels;
                viewport.Height = sizeInPixels;
                viewport.Measure(new Size(sizeInPixels, sizeInPixels));
                viewport.Arrange(new Rect(0, 0, sizeInPixels, sizeInPixels));

                RenderTargetBitmap bmp =
                    new RenderTargetBitmap
                    (sizeInPixels, sizeInPixels, 96, 96, PixelFormats.Default);
                bmp.Render(viewport);
                if (sizeInPixels > 48)
                    viewport.Children.Remove(myGridLines);
                bmp.Freeze(); // Make it available from other threads.

                return bmp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
