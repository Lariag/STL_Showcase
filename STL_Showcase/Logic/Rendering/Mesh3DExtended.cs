using STL_Showcase.Shared.Extensions;
using STL_Showcase.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace STL_Showcase.Logic.Rendering
{
    public class Mesh3DExtended : Mesh3D
    {

        #region Properties

        public Triangle[] Triangles { get; private set; }

        #endregion

        #region Constructors

        public Mesh3DExtended(SegmentedArray<Half> vertices) : base(vertices)
        {
            this.Triangles = new Triangle[TriangleCount];

            for (int i = 0, j = 0; i < TriangleCount; i++, j += 9)
            {
                Triangles[i] = new Triangle(
                    new Vertex(Vertices[j], Vertices[j + 1], Vertices[j + 2]),
                    new Vertex(Vertices[j + 3], Vertices[j + 4], Vertices[j + 5]),
                    new Vertex(Vertices[j + 6], Vertices[j + 7], Vertices[j + 8])
                    );
            }
        }
        public override void CenterObject()
        {
            if (_centered) return;
            Scale = 1f;
            OffsetX = 0f;
            OffsetY = 0f;
            OffsetZ = 0f;

            Triangle tri;
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            Action<float, float, float> updateMins = delegate (float x, float y, float z)
            {
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (z < minZ) minZ = z;
            };
            Action<float, float, float> updateMaxs = delegate (float x, float y, float z)
            {
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
                if (z > maxZ) maxZ = z;
            };


            for (int i = 0; i < Triangles.Length; i++)
            {
                tri = Triangles[i];
                updateMins(tri.v1.X, tri.v1.Y, tri.v1.Z);
                updateMaxs(tri.v1.X, tri.v1.Y, tri.v1.Z);
                updateMins(tri.v2.X, tri.v2.Y, tri.v2.Z);
                updateMaxs(tri.v2.X, tri.v2.Y, tri.v2.Z);
                updateMins(tri.v3.X, tri.v3.Y, tri.v3.Z);
                updateMaxs(tri.v3.X, tri.v3.Y, tri.v3.Z);
            }
            Scale = _ScaleFactor / (maxZ - minZ); // Fit for height.
            OffsetX = (maxX + minX) / 2f;
            OffsetY = (maxY + minY) / 2f;
            OffsetZ = minZ / 2f; // Always set on the floor (Z = 0).
            _centered = true;
        }
        #endregion

        #region Data Structures

        public class Triangle
        {
            public Vertex[] Verts { get; private set; }
            public Vertex v1 { get; set; }
            public Vertex v2 { get; set; }
            public Vertex v3 { get; set; }
            public Normal Norm { get; set; }

            public Triangle(Vertex v1, Vertex v2, Vertex v3, Normal n = null)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
                this.Norm = n;
                this.Verts = new Vertex[] { v1, v2, v3 };
            }

            public override string ToString()
            {
                return string.Format(" t[{0}] t[{1}] t[{2}] N[{3}]", v1, v2, v3, Norm);
            }
        }
        public class Vertex
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public Vertex(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }
            public override string ToString()
            {
                //return string.Format( "x{0:0.00} y{1:0.00} z{2:0.00}", X, Y, Z );
                return string.Format("{0:0.00},{1:0.00},{2:0.00}", X, Y, Z);
            }
        }
        public class Normal
        {
            public float X;
            public float Y;
            public float Z;
            public Normal(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public override string ToString()
            {
                //return string.Format( "x{0:0.00} y{1:0.00} z{2:0.00}", X, Y, Z );
                return string.Format("{0:0.00},{1:0.00},{2:0.00}", X, Y, Z);
            }
        }

        #endregion

        #region TESTS
        public static Viewport3D RenderTest(Mesh3DExtended Mesh)
        {
            Mesh.CenterObject();

            // Declare scene objects.
            Viewport3D viewport = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            PerspectiveCamera myPCamera = new PerspectiveCamera();

            // Setup camera.
            myPCamera.UpDirection = new Vector3D(0, 0, 1);
            myPCamera.Position = new Point3D(1, -1, 1.5f);
            myPCamera.LookDirection = new Vector3D(-1, 1, -1);
            myPCamera.FieldOfView = 60;
            viewport.Camera = myPCamera;

            // Lights
            DirectionalLight dirLightX = new DirectionalLight(Colors.DarkGreen, new Vector3D(-1, 0, 0));
            DirectionalLight dirLightY = new DirectionalLight(Colors.DarkRed, new Vector3D(0, 1, 0));
            DirectionalLight dirLightZ = new DirectionalLight(Colors.DarkBlue, new Vector3D(0, 0, -1));
            DirectionalLight dirLightCam = new DirectionalLight(Colors.White, new Vector3D(-1, 1, -1));

            myModel3DGroup.Children.Add(dirLightX);
            myModel3DGroup.Children.Add(dirLightY);
            myModel3DGroup.Children.Add(dirLightZ);
            myModel3DGroup.Children.Add(dirLightCam);

            // Add the mesh.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();
            Point3DCollection myPositionCollection = new Point3DCollection();
            Int32Collection myTriangleIndicesCollection = new Int32Collection();

            Mesh3DExtended.Triangle tri;
            int j = 0;
            for (int i = 0; i < Mesh.Triangles.Length; i++)
            {
                tri = Mesh.Triangles[i];
                myPositionCollection.Add(new Point3D(tri.v1.X, tri.v1.Y, tri.v1.Z));
                myPositionCollection.Add(new Point3D(tri.v2.X, tri.v2.Y, tri.v2.Z));
                myPositionCollection.Add(new Point3D(tri.v3.X, tri.v3.Y, tri.v3.Z));

                myTriangleIndicesCollection.Add(j++);
                myTriangleIndicesCollection.Add(j++);
                myTriangleIndicesCollection.Add(j++);
            }
            myMeshGeometry3D.Positions = myPositionCollection;
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            // Define material and apply to the mesh geometries.
            DiffuseMaterial myMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            myGeometryModel.Material = myMaterial;

            // Transforms for scale and position.
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new TranslateTransform3D(new Vector3D(-Mesh.OffsetX, -Mesh.OffsetY, -Mesh.OffsetZ)));
            transformGroup.Children.Add(new ScaleTransform3D(Mesh.Scale, Mesh.Scale, Mesh.Scale));

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);
            myModel3DGroup.Transform = transformGroup;

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;
            viewport.Children.Add(myModelVisual3D);

            return viewport;
        }
        #endregion
        #region Static Methods
        public static Triangle CreateTriangle(Vertex v1, Vertex v2, Vertex v3, Normal n)
        {
            return new Triangle(v1, v2, v3, n);
        }
        public static Vertex CreateVertex(float x, float y, float z)
        {
            return new Vertex(x, y, z);
        }
        public static Normal CreateNormal(float x, float y, float z)
        {
            return new Normal(x, y, z);
        }
        #endregion

    }
}
