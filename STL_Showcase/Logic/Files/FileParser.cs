using STL_Showcase.Logic.Rendering;
using STL_Showcase.Shared.Enums;
using STL_Showcase.Shared.Extensions;
using STL_Showcase.Shared.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace STL_Showcase.Logic.Files
{
    class FileParser
    {

        const string FloatStringPattern = @"([+\-]?(?:0|[1-9]\d*)(?:\.\d*)?(?:[eE][+\-]?\d+)?)";

        /// <summary>
        /// Parses the file bytes to a 3d model info.
        /// </summary>
        /// <param name="fileData">Loaded file bytes with compatible format.</param>
        /// <returns>Mesh3D object if the provided file has valid bytes and format.</returns>
        public static Mesh3D ParseModelFile(ModelFileData fileData)
        {
            switch (fileData.FileType)
            {
                case Supported3DFiles.STL_Binary:
                    return ParseSTLBinary(fileData);
                case Supported3DFiles.STL_ASCII:
                    return ParseSTLASCII(fileData);
                case Supported3DFiles.OBJ:
                    return ParseWavefront(fileData);
                default:
                    return null;
            }
        }

        public static Supported3DFiles CheckFileType(ModelFileData fileData)
        {
            if (fileData.FileName.EndsWith(".stl", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!fileData.HasBytes())
                    return Supported3DFiles.STL_Binary;

                byte[] byteBuffer = new byte[100];
                if (!fileData.ReadBytes(byteBuffer, 0, 0, byteBuffer.Length))
                    return Supported3DFiles.Unsuportted;

                // If doesnt start with "solid", its always binary.
                if (!(Encoding.ASCII.GetString(byteBuffer, 0, 5) == "solid"))
                    return Supported3DFiles.STL_Binary;

                // But some binary files also start with solid, so check the header and wish bad omen to whoever created such files.
                string header = Encoding.ASCII.GetString(byteBuffer, 0, 100);
                for (int i = 0; i < header.Length; i++)
                {
                    if (Char.IsControl(header[i]) && header[i] != '\r' && header[i] != '\n')
                        return Supported3DFiles.STL_Binary;
                }
                return Supported3DFiles.STL_ASCII;
            }
            else if (fileData.FileName.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase))
            {
                return Supported3DFiles.OBJ;
            }
            else
            {
                return Supported3DFiles.Unsuportted;
            }
        }

        static Mesh3D ParseSTLBinary(ModelFileData fileData)
        {
            // STL Binary Format: https://en.wikipedia.org/wiki/STL_(file_format)#Binary_STL

            if (!fileData.HasBytes())
                return null;

            byte[] byteBuffer = new byte[4 * 9];
            int index = 80;
            int byteBufferIndex = 0;

            fileData.ReadBytes(byteBuffer, index, 0, 4);
            int triangleAmount = (int)System.BitConverter.ToUInt32(byteBuffer, 0); index += 4;

            if (triangleAmount <= 0 || fileData.BytesLength < 80 + triangleAmount * 50)
                return null;

            SegmentedArray<int> triangles = new SegmentedArray<int>(triangleAmount * 3);
            SegmentedArray<Half> vertices = new SegmentedArray<Half>(triangleAmount * 3 * 3);
            Half[] facetNormals = new Half[triangleAmount * 3];

            try
            {
                for (int i = 0, j = 0; i < triangles.Length;)
                {
                    // Skip reading the facet normal.
                    index += 4 * 3;

                    // Triangle vertices
                    {
                        fileData.ReadBytes(byteBuffer, index, 0, byteBuffer.Length); byteBufferIndex = 0;
                        index += 4 * 9;

                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        triangles[i] = i++;

                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        triangles[i] = i++;

                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        vertices[j++] = (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                        triangles[i] = i++;
                    }

                    // Skip the remaining bytes.
                    index += 2;
                }

                return new Mesh3D(vertices, triangles, facetNormals);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        static Mesh3D ParseSTLBinaryOptimizedVerts(ModelFileData fileData)
        {
            // STL Binary Format: https://en.wikipedia.org/wiki/STL_(file_format)#Binary_STL

            if (!fileData.HasBytes())
                return null;

            byte[] byteBuffer = new byte[4 * 9];
            int index = 80;
            int byteBufferIndex = 0;

            fileData.ReadBytes(byteBuffer, index, 0, 4);
            int triangleAmount = (int)System.BitConverter.ToUInt32(byteBuffer, 0); index += 4;

            if (triangleAmount <= 0 || fileData.BytesLength < 80 + triangleAmount * 50)
                return null;

            SegmentedArray<int> triangles = new SegmentedArray<int>(triangleAmount * 3);
            SegmentedArray<Half> vertices; // = new SegmentedArray<Half>(triangleAmount * 3 * 3);
            Dictionary<Point3D, int> optimized = new Dictionary<Point3D, int>(triangleAmount * 3 * 3);
            int triangleIndex = 0;
            try
            {
                for (int i = 0, j = 0; i < triangles.Length;)
                {
                    // Skip reading the facet normal.
                    index += 4 * 3;

                    // Read the buffer
                    fileData.ReadBytes(byteBuffer, index, 0, byteBuffer.Length);
                    byteBufferIndex = 0;

                    // Triangle vertices
                    {
                        index += 4 * 9;

                        for (int k = 0; k < 3; k++)
                        {
                            float v1 = System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                            float v2 = System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                            float v3 = System.BitConverter.ToSingle(byteBuffer, byteBufferIndex); byteBufferIndex += 4;
                            Point3D point = new Point3D(v1, v2, v3);

                            if (optimized.TryGetValue(point, out int existingTriangle))
                            {
                                triangles[i++] = existingTriangle;
                            }
                            else
                            {
                                optimized.Add(point, triangleIndex);
                                triangles[i++] = triangleIndex++;
                            }
                        }

                    }

                    // Skip the remaining bytes.
                    index += 2;
                }
                vertices = new SegmentedArray<Half>(optimized.Count * 3);
                {
                    int i = 0;
                    foreach (var point in optimized)
                    {
                        vertices[i++] = (Half)point.Key.X;
                        vertices[i++] = (Half)point.Key.Y;
                        vertices[i++] = (Half)point.Key.Z;
                    }
                }
                return new Mesh3D(vertices, triangles);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        static Mesh3D ParseSTLASCII(ModelFileData fileData)
        {
            // STL ASCII Format: https://en.wikipedia.org/wiki/STL_(file_format)#Binary_STL

            if (!fileData.HasBytes())
                return null;

            List<Half> vertices = new List<Half>();

            try
            {
                Stream fileDataStream = fileData.GetStream();
                fileDataStream.Position = 0;
                StreamReader reader = new StreamReader(fileDataStream);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().TrimStart();
                    if (!line.StartsWith("vertex")) continue;

                    for (int i = 0; i < 3; i++)
                    {
                        var matches = Regex.Matches(line, FloatStringPattern);
                        vertices.Add(Half.Parse(matches[0].Value));
                        vertices.Add(Half.Parse(matches[1].Value));
                        vertices.Add(Half.Parse(matches[2].Value));
                        line = reader.ReadLine().TrimStart();
                    }

                }
            }
            catch (Exception ex)
            {
                return null;
            }

            SegmentedArray<Half> verticesArray = new SegmentedArray<Half>(vertices.Count);
            SegmentedArray<int> trianglesArray = new SegmentedArray<int>(vertices.Count / 3);
            for (int i = 0; i < vertices.Count; i++)
                verticesArray[i] = vertices[i];

            for (int i = 0; i < trianglesArray.Length; i++)
                trianglesArray[i] = i;

            return new Mesh3D(verticesArray, trianglesArray);
        }

        static Mesh3D ParseWavefront(ModelFileData fileData)
        {
            // Wavefront OBJ Format: https://en.wikipedia.org/wiki/Wavefront_.obj_file
            if (!fileData.HasBytes())
                return null;

            List<Half> vertices = new List<Half>();
            List<int> triangles = new List<int>();
            try
            {
                Stream fileDataStream = fileData.GetStream();
                fileDataStream.Position = 0;
                StreamReader reader = new StreamReader(fileDataStream);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().TrimStart();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("v "))
                    {
                        var matches = Regex.Matches(line, FloatStringPattern);
                        if (matches.Count >= 6)
                        {   // Dodge the vertex color (or is it supposed to come after x y z? Wikipedia is not clear about it).
                            vertices.Add(Half.Parse(matches[0].Value));
                            vertices.Add(Half.Parse(matches[2].Value));
                            vertices.Add(Half.Parse(matches[4].Value));
                        }
                        else
                        {
                            vertices.Add(Half.Parse(matches[0].Value));
                            vertices.Add(Half.Parse(matches[1].Value));
                            vertices.Add(Half.Parse(matches[2].Value));
                        }
                    }
                    else if (line.StartsWith("f"))
                    {
                        IEnumerable<string> faceIndexes = line.Split(' ').Where(s => s != "f" && !string.IsNullOrWhiteSpace(s));

                        int index0 = triangles.Count; // First index added for this line.
                        for (int i = 0; i < faceIndexes.Count(); i++)
                        {
                            int.TryParse(faceIndexes.ElementAt(i).Split('/').First(), out int parsedIndex);

                            // Handle the face as a triangle fan to suppor faces with more than 3 vertex (idea from https://notes.underscorediscovery.com/obj-parser-easy-parse-time-triangulation/).
                            if (i >= 3)
                            {
                                triangles.Add(triangles[index0]);
                                triangles.Add(triangles[triangles.Count - 2]);
                            }
                            triangles.Add(parsedIndex - 1); // -1 because our array starts at 0 while the OBJ starts at 1.
                        }
                    }
                    else
                        continue;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            SegmentedArray<Half> verticesArray = new SegmentedArray<Half>(vertices.Count);
            SegmentedArray<int> trianglesArray = new SegmentedArray<int>(triangles.Count);
            for (int i = 0; i < vertices.Count; i++)
                verticesArray[i] = vertices[i];

            for (int i = 0; i < triangles.Count; i++)
                trianglesArray[i] = triangles[i];

            return new Mesh3D(verticesArray, trianglesArray);
        }
    }
}
