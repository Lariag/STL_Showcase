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

        static NLog.Logger logger = NLog.LogManager.GetLogger("Parser");
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
                case Supported3DFiles._3MF:
                    return Parse3MF(fileData);
                case Supported3DFiles.OBJ:
                    return ParseWavefront(fileData);
                default:
                    return null;
            }
        }

        public static Supported3DFiles CheckFileType(ModelFileData fileData)
        {
            byte[] byteBuffer = new byte[100];

            if (fileData.HasBytes())
            {
                // Has enough bytes?
                if (!fileData.ReadBytes(byteBuffer, 0, 0, byteBuffer.Length))
                    return Supported3DFiles.Unsuportted;

                // Check if some evil being renamed some PNG file to one of the supported formats.
                if (byteBuffer[0] == 137 && byteBuffer[1] == 80 && byteBuffer[2] == 78 && byteBuffer[3] == 71 && byteBuffer[4] == 13 && byteBuffer[5] == 10 && byteBuffer[6] == 26 && byteBuffer[7] == 10)
                    return Supported3DFiles.Unsuportted;

                // Or maybe a JPG.
                if (byteBuffer[0] == 0xFF && byteBuffer[1] == 0xD8 && byteBuffer[2] == 0xFF)
                    return Supported3DFiles.Unsuportted;
            }

            if (fileData.FileName.EndsWith(".stl", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!fileData.HasBytes())
                    return Supported3DFiles.STL_Binary; // Return default STL format.

                // If doesnt start with "solid", its always binary.
                if (!(Encoding.ASCII.GetString(byteBuffer, 0, 5) == "solid"))
                    return Supported3DFiles.STL_Binary;

                // EXCEPT, that some binary files also start with solid, so check the header and wish bad omen to whoever created such files.
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
            if (fileData.FileName.EndsWith(".3mf", StringComparison.InvariantCultureIgnoreCase))
            {
                return Supported3DFiles._3MF;
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

            try
            {
                if (triangleAmount <= 0 || triangleAmount > 10000000)
                    throw new Exception("The STLBinary file has too many triangles.");

                SegmentedArray<int> triangles = new SegmentedArray<int>(triangleAmount * 3);
                SegmentedArray<Mesh3D.Vertexh> vertices = new SegmentedArray<Mesh3D.Vertexh>(triangleAmount * 3);

                for (int i = 0; i < triangles.Length;)
                {
                    // Skip reading the facet normal (why STL come with normals???).
                    index += 4 * 3;

                    // Triangle vertices
                    {
                        fileData.ReadBytes(byteBuffer, index, 0, byteBuffer.Length); byteBufferIndex = 0;
                        index += 4 * 9;

                        vertices[i] = new Mesh3D.Vertexh(
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex),
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4),
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4)
                            );
                        triangles[i] = i++;

                        vertices[i] = new Mesh3D.Vertexh(
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4),
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4),
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4)
                            );
                        triangles[i] = i++;

                        vertices[i] = new Mesh3D.Vertexh(
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4),
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4),
                            (Half)System.BitConverter.ToSingle(byteBuffer, byteBufferIndex += 4)
                            );
                        triangles[i] = i++;
                    }

                    // Skip the remaining bytes.
                    index += 2;
                }

                return new Mesh3D(vertices, triangles);
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception when processing file: {FileName}, {FileSizeKB}, {FileType}", fileData.FileName, fileData.FileSizeKB, fileData.FileType.ToString());
                return null;
            }
        }

        static Mesh3D ParseSTLASCII(ModelFileData fileData)
        {
            // STL ASCII Format: https://en.wikipedia.org/wiki/STL_(file_format)#Binary_STL

            if (!fileData.HasBytes())
                return null;

            List<Mesh3D.Vertexh> vertices = new List<Mesh3D.Vertexh>();

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
                        vertices.Add(new Mesh3D.Vertexh(
                            Half.Parse(matches[0].Value),
                            Half.Parse(matches[1].Value),
                            Half.Parse(matches[2].Value)
                            ));
                        line = reader.ReadLine().TrimStart();
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Trace(ex, "Exception when processing file: {FileName}, {FileSizeKB}, {FileType}", fileData.FileName, fileData.FileSizeKB, fileData.FileType.ToString());
                return null;
            }

            SegmentedArray<Mesh3D.Vertexh> verticesArray = new SegmentedArray<Mesh3D.Vertexh>(vertices.Count);
            SegmentedArray<int> trianglesArray = new SegmentedArray<int>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                verticesArray[i] = vertices[i];
                trianglesArray[i] = i;
            }
            return new Mesh3D(verticesArray, trianglesArray);
        }

        static Mesh3D ParseWavefront(ModelFileData fileData)
        {
            // Wavefront OBJ Format: https://en.wikipedia.org/wiki/Wavefront_.obj_file
            if (!fileData.HasBytes())
                return null;

            List<Mesh3D.Vertexh> vertices = new List<Mesh3D.Vertexh>();
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
                            vertices.Add(new Mesh3D.Vertexh(
                                Half.Parse(matches[0].Value),
                                Half.Parse(matches[2].Value),
                                Half.Parse(matches[4].Value)
                                ));
                        }
                        else
                        {
                            vertices.Add(new Mesh3D.Vertexh(
                                Half.Parse(matches[0].Value),
                                Half.Parse(matches[1].Value),
                                Half.Parse(matches[2].Value)
                                ));
                        }
                    }
                    else if (line.StartsWith("f"))
                    {
                        IEnumerable<string> faceIndexes = line.Split(' ').Where(s => s != "f" && !string.IsNullOrWhiteSpace(s));

                        int index0 = triangles.Count; // First index added for this line.
                        for (int i = 0; i < faceIndexes.Count(); i++)
                        {
                            int.TryParse(faceIndexes.ElementAt(i).Split('/').First(), out int parsedIndex);

                            // Handle the face as a triangle fan to support faces with more than 3 vertex (idea from https://notes.underscorediscovery.com/obj-parser-easy-parse-time-triangulation/).
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
                logger.Trace(ex, "Exception when processing file: {FileName}, {FileSizeKB}, {FileType}", fileData.FileName, fileData.FileSizeKB, fileData.FileType.ToString());
                return null;
            }

            SegmentedArray<Mesh3D.Vertexh> verticesArray = new SegmentedArray<Mesh3D.Vertexh>(vertices.Count);
            SegmentedArray<int> trianglesArray = new SegmentedArray<int>(triangles.Count);
            for (int i = 0; i < vertices.Count; i++)
                verticesArray[i] = vertices[i];

            for (int i = 0; i < triangles.Count; i++)
                trianglesArray[i] = triangles[i];

            return new Mesh3D(verticesArray, trianglesArray);
        }

        static Mesh3D Parse3MF(ModelFileData filedData)
        {
            // 3MF (3D Manufacturing Format): https://github.com/3MFConsortium/spec_core/blob/master/3MF%20Core%20Specification.md
            // File loading using Lib3MF: https://github.com/3MFConsortium/lib3mf

            List<int> trianglesList = new List<int>();
            List<Mesh3D.Vertexh> vertexList = new List<Mesh3D.Vertexh>();

            Lib3MF.CModel model = Lib3MF.Wrapper.CreateModel();
            model.QueryReader("3mf").ReadFromFile(filedData.FileFullPath);

            var meshIterator = model.GetMeshObjects();
            int lastMeshVertexIndex = 0;

            while (meshIterator.MoveNext())
            {
                var resource = meshIterator.GetCurrent();

                Lib3MF.CMeshObject meshobject = model.GetMeshObjectByID(resource.GetResourceID());

                int triangleAmount = (int)meshobject.GetTriangleCount();
                if (triangleAmount == 0)
                    continue;

                for (int i = 0; i < triangleAmount; i++)
                {
                    var triangle = meshobject.GetTriangle((uint)i);

                    int index0 = trianglesList.Count;
                    for (int j = 0; j < triangle.Indices.Length; j++)
                    {
                        if (j >= 3)
                        {  // Trick to triangulate polygons with more than 3 vertex.
                            trianglesList.Add(trianglesList[index0]);
                            trianglesList.Add(trianglesList[trianglesList.Count - 2]);
                        }
                        trianglesList.Add(lastMeshVertexIndex + (int)triangle.Indices[j]);
                    }
                }

                var vertexAmount = meshobject.GetVertexCount();
                for (int i = 0; i < vertexAmount; i++)
                {
                    var vertex = meshobject.GetVertex((uint)i);
                    vertexList.Add(new Mesh3D.Vertexh((Half)vertex.Coordinates[0], (Half)vertex.Coordinates[1], (Half)vertex.Coordinates[2]));
                }

                lastMeshVertexIndex = vertexList.Count;
            }

            if (trianglesList.Count == 0 || vertexList.Count == 0)
                return null;

            SegmentedArray<Mesh3D.Vertexh> verticesArray = new SegmentedArray<Mesh3D.Vertexh>(vertexList.Count);
            SegmentedArray<int> trianglesArray = new SegmentedArray<int>(trianglesList.Count);
            for (int i = 0; i < vertexList.Count; i++)
                verticesArray[i] = vertexList[i];

            for (int i = 0; i < trianglesList.Count; i++)
                trianglesArray[i] = trianglesList[i];

            return new Mesh3D(verticesArray, trianglesArray);
        }
    }
}
