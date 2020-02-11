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
        protected bool _centered = false;

        private SegmentedArray<Half> _Vertices;
        public SegmentedArray<Half> Vertices {
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

        public Mesh3D(SegmentedArray<Half> vertices, SegmentedArray<int> triangles, Half[] faceNormals = null)
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
            //for (int i = 0; i < TriangleCount; i += 9)
            //{
            //    if (_Vertices[i] < minX) minX = _Vertices[i];
            //    if (_Vertices[i] > maxX) maxX = _Vertices[i];
            //    if (_Vertices[i + 1] < minY) minY = _Vertices[i + 1];
            //    if (_Vertices[i + 1] > maxY) maxY = _Vertices[i + 1];
            //    if (_Vertices[i + 2] < minZ) minZ = _Vertices[i + 2];
            //    if (_Vertices[i + 2] > maxZ) maxZ = _Vertices[i + 2];

            //    if (_Vertices[i + 3] < minX) minX = _Vertices[i + 3];
            //    if (_Vertices[i + 3] > maxX) maxX = _Vertices[i + 3];
            //    if (_Vertices[i + 4] < minY) minY = _Vertices[i + 4];
            //    if (_Vertices[i + 4] > maxY) maxY = _Vertices[i + 4];
            //    if (_Vertices[i + 5] < minZ) minZ = _Vertices[i + 5];
            //    if (_Vertices[i + 5] > maxZ) maxZ = _Vertices[i + 5];

            //    if (_Vertices[i + 6] < minX) minX = _Vertices[i + 6];
            //    if (_Vertices[i + 6] > maxX) maxX = _Vertices[i + 6];
            //    if (_Vertices[i + 7] < minY) minY = _Vertices[i + 7];
            //    if (_Vertices[i + 7] > maxY) maxY = _Vertices[i + 7];
            //    if (_Vertices[i + 8] < minZ) minZ = _Vertices[i + 8];
            //    if (_Vertices[i + 8] > maxZ) maxZ = _Vertices[i + 8];

            //    i += triangleSkip * 9;
            //}

            for (int i = 0; i < Triangles.Length - 2; i++)
            {
                if (_Vertices[Triangles[i] * 3] < minX) minX = _Vertices[Triangles[i] * 3];
                if (_Vertices[Triangles[i] * 3] > maxX) maxX = _Vertices[Triangles[i] * 3];
                if (_Vertices[Triangles[i] * 3 + 1] < minY) minY = _Vertices[Triangles[i] * 3 + 1];
                if (_Vertices[Triangles[i] * 3 + 1] > maxY) maxY = _Vertices[Triangles[i] * 3 + 1];
                if (_Vertices[Triangles[i] * 3 + 2] < minZ) minZ = _Vertices[Triangles[i] * 3 + 2];
                if (_Vertices[Triangles[i] * 3 + 2] > maxZ) maxZ = _Vertices[Triangles[i] * 3 + 2];

                Console.WriteLine(_Vertices[Triangles[i]] + " " + _Vertices[Triangles[i] + 1] + " " + _Vertices[Triangles[i] + 2]);
                i += triangleSkip * 9;
            }

            Scale = _ScaleFactor / Distance(minX, minY, minZ, maxX, maxY, maxZ);
            OffsetX = (maxX + minX) / 2f;
            OffsetY = (maxY + minY) / 2f; // minY + (maxY - minY) / 2f
            OffsetZ = minZ; // Set on the floor (Z = 0).

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
                vertexNormals = new Half[this.Vertices.Length];
                int x = 0;
                int y = 1;
                int z = 2;
                Half[] v1 = new Half[3];
                Half[] v2 = new Half[3];

                int tri1;
                int tri2;
                int tri3;

                float vx, vy, vz;

                for (int i = 0; i < Triangles.Length; i += 3)
                {
                    tri1 = Triangles[i] * 3;
                    tri2 = Triangles[i + 1] * 3;
                    tri3 = Triangles[i + 2] * 3;

                    v1[x] = Vertices[tri1 + x] - Vertices[tri2 + x];
                    v1[y] = Vertices[tri1 + y] - Vertices[tri2 + y];
                    v1[z] = Vertices[tri1 + z] - Vertices[tri2 + z];

                    v2[x] = Vertices[tri2 + x] - Vertices[tri3 + x];
                    v2[y] = Vertices[tri2 + y] - Vertices[tri3 + y];
                    v2[z] = Vertices[tri2 + z] - Vertices[tri3 + z];

                    vx = v1[y] * v2[z] - v1[z] * v2[y];
                    vy = v1[z] * v2[x] - v1[x] * v2[z];
                    vz = v1[x] * v2[y] - v1[y] * v2[x];
                    NormalizeVector(ref vx, ref vy, ref vz);

                    vertexNormals[tri1 + x] = vertexNormals[tri2 + x] = vertexNormals[tri3 + x] = (Half)vx;
                    vertexNormals[tri1 + y] = vertexNormals[tri2 + y] = vertexNormals[tri3 + y] = (Half)vy;
                    vertexNormals[tri1 + z] = vertexNormals[tri2 + z] = vertexNormals[tri3 + z] = (Half)vz;
                }
            }
        }

        public Half[] GetVertexArray()
        {
            Half[] v = new Half[this.Vertices.Length * 2];

            int x = 0;
            int y = 1;
            int z = 2;

            Half[] v1 = new Half[3];
            Half[] v2 = new Half[3];

            for (int i = 0, j = 0; i < Vertices.Length; i += 9, j += 18)
            {
                v[j] = Vertices[i];
                v[j + 1] = Vertices[i + 1];
                v[j + 2] = Vertices[i + 2];

                v[j + 6] = Vertices[i + 3];
                v[j + 7] = Vertices[i + 4];
                v[j + 8] = Vertices[i + 5];

                v[j + 12] = Vertices[i + 6];
                v[j + 13] = Vertices[i + 7];
                v[j + 14] = Vertices[i + 8];



                v1[x] = v[j + 0] - v[j + 6];
                v1[y] = v[j + 1] - v[j + 7];
                v1[z] = v[j + 2] - v[j + 8];

                v2[x] = v[j + 6] - v[j + 12];
                v2[y] = v[j + 7] - v[j + 13];
                v2[z] = v[j + 8] - v[j + 14];

                OpenTK.Vector3 v3 = new OpenTK.Vector3(v1[y] * v2[z] - v1[z] * v2[y], v1[z] * v2[x] - v1[x] * v2[z], v1[x] * v2[y] - v1[y] * v2[x]);
                v3.Normalize();

                v[j + 3] = v[j + 9] = v[j + 15] = (Half)v3.X;// v1[y] * v2[z] - v1[z] * v2[y];
                v[j + 4] = v[j + 10] = v[j + 16] = (Half)v3.Y;//v1[z] * v2[x] - v1[x] * v2[z];
                v[j + 5] = v[j + 11] = v[j + 17] = (Half)v3.Z;//v1[x] * v2[y] - v1[y] * v2[x];



            }
            return v;
        }

        private static void NormalizeVector(ref float x, ref float y, ref float z)
        {
            float distance = (float)Math.Sqrt(x * x + y * y + z * z);
            x = x / distance;
            y = y / distance;
            z = z / distance;
        }
        private static void NormalizeVector(ref Half x, ref Half y, ref Half z)
        {
            Half distance = (Half)Math.Sqrt(x * x + y * y + z * z);
            x = x / distance;
            y = y / distance;
            z = z / distance;
        }
        private static float Distance(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2));
        }

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
