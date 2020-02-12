using STL_Showcase.Shared.Extensions;
using STL_Showcase.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Logic.Rendering
{

    /// <summary>
    /// Stores the most basic information for 3D objects.
    /// Provides a center/escale method.
    /// </summary>
    public class Mesh3D
    {
        protected const float _ScaleFactor = 2f;
        protected const int _RepresentativeSalmpleTriangles = 2000;

        public float Scale { get; protected set; }
        public float OffsetX { get; protected set; }
        public float OffsetY { get; protected set; }
        public float OffsetZ { get; protected set; }
        public float OffsetZCentered { get; protected set; }

        protected bool _centered = false;

        private SegmentedArray<Vertexh> _Vertices;
        public SegmentedArray<Vertexh> Vertices {
            get { return _Vertices; }
            set
            {
                _Vertices = value;
                if (this.Triangles == null) TriangleCount = value.Length / 3;
                _centered = false;
            }
        }
        private SegmentedArray<int> _Triangles; // Index of the triangles in the vertices array.
        public SegmentedArray<int> Triangles {
            get { return _Triangles; }
            set
            {
                _Triangles = value; TriangleCount = value.Length / 3; _centered = false;
                _centered = false;
            }
        }
        public int TriangleCount { get; private set; }

        public Half[] vertexNormals; // NOTE: No data for normals. Re-note: Now data for normals!
        public bool HasFaceNormals {
            get { return vertexNormals != null; }
        }

        public Mesh3D(SegmentedArray<Vertexh> vertices, SegmentedArray<int> triangles, Half[] faceNormals = null)
        {
            this.Vertices = vertices;
            this.Triangles = triangles;
            this.vertexNormals = faceNormals;

            Scale = 1f;
            OffsetX = 0f;
            OffsetY = 0f;
            OffsetZ = 0f;
        }

        public virtual void CenterObject()
        {
            if (_centered) return;
            Scale = 1f;
            OffsetX = 0f;
            OffsetY = 0f;
            OffsetZ = 0f;

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            int triangleSkip = (TriangleCount > _RepresentativeSalmpleTriangles ? TriangleCount / _RepresentativeSalmpleTriangles : 0);
            
            for (int i = 0; i < Triangles.Length; i++)
            {
                if (_Vertices[Triangles[i]].x < minX) minX = _Vertices[Triangles[i]].x;
                if (_Vertices[Triangles[i]].x > maxX) maxX = _Vertices[Triangles[i]].x;
                if (_Vertices[Triangles[i]].y < minY) minY = _Vertices[Triangles[i]].y;
                if (_Vertices[Triangles[i]].y > maxY) maxY = _Vertices[Triangles[i]].y;
                if (_Vertices[Triangles[i]].z < minZ) minZ = _Vertices[Triangles[i]].z;
                if (_Vertices[Triangles[i]].z > maxZ) maxZ = _Vertices[Triangles[i]].z;

                i += triangleSkip * 9;
            }

            Scale = _ScaleFactor / Distance(minX, minY, minZ, maxX, maxY, maxZ);
            OffsetX = (maxX + minX) / 2f;
            OffsetY = (maxY + minY) / 2f;
            OffsetZ = minZ; // Set on the floor (Z = 0).
            OffsetZCentered = (maxZ + minZ) / 2f; // Set center of the object on the floor.
            _centered = true;
        }

        public uint[] GetTrianglesUint(bool includeCompleteVertexIndexes)
        {
            if (!includeCompleteVertexIndexes)
                return this.Triangles.Select(t => (uint)t).ToArray();
            else
            {
                uint[] fullIndex = new uint[this.Triangles.Length * 3];
                for (int i = 0, j = 0; i < this.Triangles.Length; i++, j += 3)
                {
                    fullIndex[j] = (uint)this.Triangles[i];
                    fullIndex[j + 1] = fullIndex[j] + 1;
                    fullIndex[j + 2] = fullIndex[j] + 2;
                }
                return fullIndex;
            }
        }

        public int[] GetTrianglesComplete()
        {
            int[] fullIndex = new int[this.Triangles.Length * 3];
            for (int i = 0, j = 0; i < this.Triangles.Length; i++, j += 3)
            {
                fullIndex[j] = this.Triangles[i];
                fullIndex[j + 1] = fullIndex[j] + 1;
                fullIndex[j + 2] = fullIndex[j] + 2;
            }
            return fullIndex;
        }

        public void CalculateFaceNormals(bool forceRecalculation)
        {
            if (!HasFaceNormals || forceRecalculation)
            {
                vertexNormals = new Half[this.Vertices.Length * 3];
                int x = 0;
                int y = 1;
                int z = 2;
                Vertexh v1;
                Vertexh v2;

                int tri1;
                int tri2;
                int tri3;

                float vx, vy, vz;

                for (int i = 0; i < Triangles.Length; i += 3)
                {
                    tri1 = Triangles[i];
                    tri2 = Triangles[i + 1];
                    tri3 = Triangles[i + 2];

                    v1 = new Vertexh(
                         Vertices[tri1].x - Vertices[tri2].x,
                         Vertices[tri1].y - Vertices[tri2].y,
                         Vertices[tri1].z - Vertices[tri2].z
                        );

                    v2 = new Vertexh(
                        Vertices[tri2].x - Vertices[tri3].x,
                        Vertices[tri2].y - Vertices[tri3].y,
                        Vertices[tri2].z - Vertices[tri3].z
                        );

                    vx = v1.y * v2.z - v1.z * v2.y;
                    vy = v1.z * v2.x - v1.x * v2.z;
                    vz = v1.x * v2.y - v1.y * v2.x;
                    NormalizeVector(ref vx, ref vy, ref vz);

                    vertexNormals[tri1 * 3 + x] = vertexNormals[tri2 * 3 + x] = vertexNormals[tri3 * 3 + x] = (Half)vx;
                    vertexNormals[tri1 * 3 + y] = vertexNormals[tri2 * 3 + y] = vertexNormals[tri3 * 3 + y] = (Half)vy;
                    vertexNormals[tri1 * 3 + z] = vertexNormals[tri2 * 3 + z] = vertexNormals[tri3 * 3 + z] = (Half)vz;
                }
            }
        }

        private static void NormalizeVector(ref float x, ref float y, ref float z)
        {
            float distance = (float)Math.Sqrt(x * x + y * y + z * z);
            x = x / distance;
            y = y / distance;
            z = z / distance;
        }

        private static float Distance(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2));
        }

        #region Structures

        public struct Vertexf
        {
            public Vertexf(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public float x;
            public float y;
            public float z;

            public const int SizeInBytes = 12;
        }

        public struct Vertexh
        {
            public Vertexh(Half x, Half y, Half z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public Half x;
            public Half y;
            public Half z;

            public const int SizeInBytes = 6;
        }

        #endregion Structures

        //public bool OptimizeTriangleCount(int maxTriangles)
        //{
        //    if (maxTriangles >= TriangleCount)
        //        return true;

        //    List<int> indexesToRemove = new List<int>();
        //    List<Half> newVertices = new List<Half>();
        //    int verticeAmountToRemove = 0;

        //    // Determine vertices to merge.
        //    int iterations = (TriangleCount - maxTriangles) / 2;
        //    int removeEvery = Triangles.Length / iterations;
        //    for (int i = 0; i < Triangles.Length - 1; i += removeEvery)
        //    {
        //        int removalIndex = i - (i % 3);
        //        indexesToRemove.Add(removalIndex);
        //        indexesToRemove.Add(removalIndex + 1);
        //    }
        //    indexesToRemove = indexesToRemove.OrderBy(v => v).ToList();
        //    // Determine vertices to create.
        //    for (int i = 0; i < indexesToRemove.Count; i += 2)
        //    {
        //        newVertices.Add((indexesToRemove[i] + indexesToRemove[i + 3]) / 2);
        //        newVertices.Add((indexesToRemove[i + 1] + indexesToRemove[i + 4]) / 2);
        //        newVertices.Add((indexesToRemove[i + 2] + indexesToRemove[i + 5]) / 2);
        //    }
        //    verticeAmountToRemove = indexesToRemove.Count * 3 - newVertices.Count;
        //    // New vertices array, removing excluded ones with new ones.
        //    SegmentedArray<Half> finalVertices = new SegmentedArray<Half>(verticeAmountToRemove);
        //    for (int i = 0;)
        //        return true;
        //}
    }
}
