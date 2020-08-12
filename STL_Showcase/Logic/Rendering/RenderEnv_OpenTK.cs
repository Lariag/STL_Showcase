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

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using SDPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using OpenTK;
using STL_Showcase.Shared.Util;
using STL_Showcase.Shared.Main;

namespace STL_Showcase.Logic.Rendering
{
    class RenderEnv_OpenTK : IRenderEnviorement
    {
        GameWindow window;
        GraphicsMode graphicMode;
        Mesh3D mesh;

        int sizeInPixels;

        float[] floorGridVertex; // vertex + normals
        int[] floorGridIndexes; // Triangle indexes
        int floorGridVertexBuffer;
        int floorGridIndexBuffer;

        int modelVertexBuffer;
        int modelNormalBuffer;
        int modelIndexBuffer;

        Matrix4 modelViewMatrix;
        Matrix4 projectionMatrix;

        float[] mat_diffuse1;
        float[] mat_diffuse2;
        float[] mat_diffuse3;
        float[] mat_diffuse4;

        float[] light_position1;
        float[] light_position2;
        float[] light_position3;
        float[] light_position4;


        public RenderEnv_OpenTK(int sizeInPixels)
        {
            this.sizeInPixels = sizeInPixels;

            graphicMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 1);
            window = new OpenTK.GameWindow(sizeInPixels, sizeInPixels, graphicMode, "", OpenTK.GameWindowFlags.FixedWindow, OpenTK.DisplayDevice.Default, 3, 0, GraphicsContextFlags.Offscreen);
            window.WindowBorder = WindowBorder.Hidden;
            window.ClientRectangle = new Rectangle(0, 0, sizeInPixels, sizeInPixels);
            //window.Visible = false;
            window.MakeCurrent();

            modelViewMatrix = Matrix4.LookAt(2f, 2f, 2f, 0, 0, 0f, 0, 0.0f, -1.0f);
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(0.60f, 1f, 0.01f, 10f);

            GL.Enable(EnableCap.DepthTest);
        }
        public void SetEnviorementOptions(RenderAspectEnum renderAspect)
        {
            GL.Disable(EnableCap.Light0);
            GL.Disable(EnableCap.Light1);
            GL.Disable(EnableCap.Light2);
            GL.Disable(EnableCap.Light3);

            // InitializeFloorGrid();

            //// Lighting
            {
                float highLight = 1f;

                mat_diffuse1 = null;
                mat_diffuse2 = null;
                mat_diffuse3 = null;
                mat_diffuse4 = null;

                if (DefaultFactory.GetDefaultUserSettings().GetSettingBool(UserSettingEnum.EnableThumnailColorsByShaders))
                {
                    mat_diffuse1 = new float[] { 0f, 1f, 0f };
                    light_position1 = new float[] { 1f, 0f, 0f, 0f };

                    mat_diffuse2 = new float[] { 0f, 0f, 1f };
                    light_position2 = new float[] { 0f, 0f, 1f, 0f };

                    mat_diffuse3 = new float[] { 1f, 0f, 0f };
                    light_position3 = new float[] { 0f, -1f, 0f, 0f };
                }
                else
                {

                    if (renderAspect == RenderAspectEnum.VioletBlue)
                    {
                        mat_diffuse1 = new float[] { Colors.Violet.R / 256f, Colors.Violet.G / 256f, Colors.Violet.B / 256f };
                        light_position1 = new float[] { 0.5f, -1f, 0.5f, 0f };

                        mat_diffuse2 = new float[] { Colors.Blue.R / 256f, Colors.Blue.G / 256f, Colors.Blue.B / 256f };
                        light_position2 = new float[] { 1f, -0.5f, 0.5f, 0f };
                    }
                    else if (renderAspect == RenderAspectEnum.RedOrangeYellow)
                    {
                        mat_diffuse1 = new float[] { 1f, 0f, 0f };
                        light_position1 = new float[] { 1f, 0f, 0f, 0f };

                        mat_diffuse2 = new float[] { 1, 1f, 0f };
                        light_position2 = new float[] { -0f, -1f, 0f, 0f };

                        mat_diffuse3 = new float[] { Colors.Orange.R / 256f, Colors.Orange.G / 256f, Colors.Orange.B / 256f };
                        light_position3 = new float[] { 0f, 0f, 1f, 0f };
                    }
                    else if (renderAspect == RenderAspectEnum.GreenLimeYellow)
                    {
                        mat_diffuse1 = new float[] { 0f, 0.5f, 0f };
                        light_position1 = new float[] { 1f, 0f, 0f, 0f };

                        mat_diffuse2 = new float[] { 1, 1f, 0f };
                        light_position2 = new float[] { -0f, -1f, 0f, 0f };

                        mat_diffuse3 = new float[] { Colors.LimeGreen.R / 256f, Colors.LimeGreen.G / 256f, Colors.LimeGreen.B / 256f };
                        light_position3 = new float[] { 0f, 0f, 1f, 0f };
                    }
                    else if (renderAspect == RenderAspectEnum.PinkFucsiaViolet)
                    {
                        mat_diffuse1 = new float[] { 1f, 0.75f, 0.795f };
                        light_position1 = new float[] { 1f, 0f, 0f, 0f };

                        mat_diffuse2 = new float[] { 0.58f, 0.155f, 0.31f };
                        light_position2 = new float[] { -0f, -1f, 0f, 0f };

                        mat_diffuse3 = new float[] { 0.93f, 0.51f, 0.93f };
                        light_position3 = new float[] { 0f, 0f, 1f, 0f };
                    }
                    else if (renderAspect == RenderAspectEnum.CyanBlue)
                    {
                        mat_diffuse1 = new float[] { 0f, 0f, 1f };
                        light_position1 = new float[] { 1f, 0f, 0f, 0f };

                        mat_diffuse2 = new float[] { 0.39f, 0.58f, 0.92f };
                        light_position2 = new float[] { -0f, -1f, 0f, 0f };

                        mat_diffuse3 = new float[] { 0f, 1f, 1f };
                        light_position3 = new float[] { 0f, 0f, 1f, 0f };
                    }
                    else if (renderAspect == RenderAspectEnum.RedRedish)
                    {
                        mat_diffuse1 = new float[] { 1f, 0f, 0f };
                        light_position1 = new float[] { 1f, 0f, 0f, 0f };

                        mat_diffuse2 = new float[] { 0.5f, 0f, 0f };
                        light_position2 = new float[] { -0f, -1f, 0f, 0f };

                        mat_diffuse3 = new float[] { 0.85f, 0.44f, 0.57f };
                        light_position3 = new float[] { 0f, 0f, 1f, 0f };
                    }
                    else if (renderAspect == RenderAspectEnum.Yellow)
                    {
                        mat_diffuse1 = new float[] { 1f, 1f, 0f };
                        light_position1 = new float[] { 1f, 0f, 0f, 0f };

                        mat_diffuse2 = new float[] { 0.6f, 0.8f, 0.195f };
                        light_position2 = new float[] { -0f, -1f, 0f, 0f };

                        mat_diffuse3 = new float[] { 1f, 1f, 0.87f };
                        light_position3 = new float[] { 0f, 0f, 1f, 0f };
                    }
                    else
                    {
                        if (renderAspect == RenderAspectEnum.PerNormal)
                        {
                            mat_diffuse1 = new float[] { 0f, 0.39f, 0f };
                            light_position1 = new float[] { 1f, 0f, 0f, 0f };

                            mat_diffuse2 = new float[] { 0f, 0f, 0.54f };
                            light_position2 = new float[] { 0f, 0f, 1f, 0f };

                            mat_diffuse3 = new float[] { 0.54f, 0f, 0f };
                            light_position3 = new float[] { 0f, -1f, 0f, 0f };
                        }

                        mat_diffuse4 = new float[] { highLight, highLight, highLight };
                        light_position4 = new float[] { 1f, -1f, 1f, 0f };
                    }
                }
            }
        }
        public void SetModel(Mesh3D mesh)
        {
            this.mesh = mesh;

            // Create buffers
            {
                GL.GenBuffers(1, out modelVertexBuffer);
                GL.BindBuffer(BufferTarget.ArrayBuffer, modelVertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * Mesh3D.Vertexh.SizeInBytes, mesh.Vertices.ToArray(), BufferUsageHint.StaticDraw);

                GL.GenBuffers(1, out modelIndexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, modelIndexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Triangles.Length * sizeof(int), mesh.Triangles.ToArray(), BufferUsageHint.StaticDraw);
                GL.VertexPointer(3, VertexPointerType.HalfFloat, 0, new IntPtr(0));

                GL.GenBuffers(1, out modelNormalBuffer);
                GL.BindBuffer(BufferTarget.ArrayBuffer, modelNormalBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertexNormals.Length * Shared.Extensions.Half.SizeInBytes, mesh.vertexNormals, BufferUsageHint.StaticDraw);
                GL.NormalPointer(NormalPointerType.HalfFloat, 0, new IntPtr(0));
            }
        }
        public void RemoveModel()
        {
            this.mesh = null;
        }
        public BitmapSource RenderImage()
        {
            // float newScale = (mesh.Scale < 0.001 ? mesh.Scale * 10 : (mesh.Scale > 0.1 ? mesh.Scale * 0.1f : mesh.Scale)) * 100f;

            // Initialize Render
            {
                GL.ClearColor(1.0f, 1.0f, 1.0f, 0.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref projectionMatrix);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref modelViewMatrix);

                GL.Enable(EnableCap.RescaleNormal);

                GL.Translate(-mesh.OffsetX * mesh.Scale, mesh.OffsetY * mesh.Scale, -mesh.OffsetZCentered * mesh.Scale);
                GL.Scale(mesh.Scale, -mesh.Scale, mesh.Scale);

                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.NormalArray);
            }


            // Enable Lighting
            {
                GL.Enable(EnableCap.Lighting);
                GL.ShadeModel(ShadingModel.Smooth);

                if (mat_diffuse1 != null)
                {
                    GL.Light(LightName.Light0, LightParameter.Position, light_position1);
                    GL.Light(LightName.Light0, LightParameter.Diffuse, mat_diffuse1);
                    GL.Enable(EnableCap.Light0);
                }
                if (mat_diffuse2 != null)
                {
                    GL.Light(LightName.Light1, LightParameter.Position, light_position2);
                    GL.Light(LightName.Light1, LightParameter.Diffuse, mat_diffuse2);
                    GL.Enable(EnableCap.Light1);
                }
                if (mat_diffuse3 != null)
                {
                    GL.Light(LightName.Light2, LightParameter.Position, light_position3);
                    GL.Light(LightName.Light2, LightParameter.Diffuse, mat_diffuse3);
                    GL.Enable(EnableCap.Light2);
                }
                if (mat_diffuse4 != null)
                {
                    GL.Light(LightName.Light3, LightParameter.Position, light_position4);
                    GL.Light(LightName.Light3, LightParameter.Diffuse, mat_diffuse4);
                    GL.Enable(EnableCap.Light3);
                }
            }

            // Draw model
            GL.DrawElements(BeginMode.Triangles, mesh.Triangles.Length, DrawElementsType.UnsignedInt, 0);

            GL.Disable(EnableCap.Lighting);

            // Finish
            {
                GL.DisableClientState(ArrayCap.VertexArray);
                GL.DisableClientState(ArrayCap.NormalArray);
                GL.Flush();
            }

            // Generate image
            using (var bmp = new Bitmap(sizeInPixels, sizeInPixels, SDPixelFormat.Format32bppArgb))
            {
                var bmpData = bmp.LockBits(new Rectangle(0, 0, sizeInPixels, sizeInPixels), ImageLockMode.WriteOnly, SDPixelFormat.Format32bppArgb);
                GL.PixelStore(PixelStoreParameter.PackRowLength, bmpData.Stride / 4);
                GL.ReadPixels(0, 0, sizeInPixels, sizeInPixels, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                bmp.UnlockBits(bmpData);

                bmpData = bmp.LockBits(new Rectangle(0, 0, sizeInPixels, sizeInPixels), ImageLockMode.ReadOnly, SDPixelFormat.Format32bppArgb);

                var bitmapSource = BitmapSource.Create(
                    bmpData.Width, bmpData.Height,
                    bmp.HorizontalResolution, bmp.VerticalResolution,
                    PixelFormats.Bgra32, null,
                    bmpData.Scan0, bmpData.Stride * bmpData.Height, bmpData.Stride);
                bitmapSource.Freeze();
                bmp.UnlockBits(bmpData);

                return bitmapSource;
            }
        }

        private void InitializeFloorGrid()
        {
            int vertexPerLine = 4; // 1 quad per line.
            int linesAmount = 500; // 500 + 500 lines.

            float lineWidht = 0.02f / 2f;
            float lineLength = linesAmount;

            floorGridVertex = new float[vertexPerLine * 3 * 2 * (linesAmount + linesAmount)];
            floorGridIndexes = new int[(linesAmount + linesAmount) * vertexPerLine];

            int vIndex = 0;
            int vIndexAdd = vertexPerLine * 3 * 2 * linesAmount;

            for (int i = 0; i < linesAmount; i += 1, vIndex += 24)
            {
                floorGridVertex[vIndex] = -lineWidht;
                floorGridVertex[vIndex + 1] = 0;
                floorGridVertex[vIndex + 2] = 0;

                floorGridVertex[vIndex + 3] = 0f;
                floorGridVertex[vIndex + 4] = 0f;
                floorGridVertex[vIndex + 5] = 1f;



                floorGridVertex[vIndex + 6] = lineWidht;
                floorGridVertex[vIndex + 7] = 0;
                floorGridVertex[vIndex + 8] = 0;

                floorGridVertex[vIndex + 9] = 0f;
                floorGridVertex[vIndex + 10] = 0f;
                floorGridVertex[vIndex + 11] = 1f;



                floorGridVertex[vIndex + 12] = lineWidht;
                floorGridVertex[vIndex + 13] = lineLength;
                floorGridVertex[vIndex + 14] = 0;

                floorGridVertex[vIndex + 15] = 0f;
                floorGridVertex[vIndex + 16] = 0f;
                floorGridVertex[vIndex + 17] = 1f;



                floorGridVertex[vIndex + 18] = -lineWidht;
                floorGridVertex[vIndex + 19] = lineLength;
                floorGridVertex[vIndex + 20] = 0;

                floorGridVertex[vIndex + 21] = 0f;
                floorGridVertex[vIndex + 22] = 0f;
                floorGridVertex[vIndex + 23] = 1f;
            }

            for (int i = 0; i < linesAmount; i += 1)
            {
                floorGridVertex[vIndex] = 0;
                floorGridVertex[vIndex + 1] = -lineWidht;
                floorGridVertex[vIndex + 2] = 0;

                floorGridVertex[vIndex + 3] = 0f;
                floorGridVertex[vIndex + 4] = 0f;
                floorGridVertex[vIndex + 5] = 1f;



                floorGridVertex[vIndex + 6] = 0;
                floorGridVertex[vIndex + 7] = lineWidht;
                floorGridVertex[vIndex + 8] = 0;

                floorGridVertex[vIndex + 9] = 0f;
                floorGridVertex[vIndex + 10] = 0f;
                floorGridVertex[vIndex + 11] = 1f;



                floorGridVertex[vIndex + 12] = lineLength;
                floorGridVertex[vIndex + 13] = lineWidht;
                floorGridVertex[vIndex + 14] = 0;

                floorGridVertex[vIndex + 15] = 0f;
                floorGridVertex[vIndex + 16] = 0f;
                floorGridVertex[vIndex + 17] = 1f;



                floorGridVertex[vIndex + 18] = lineLength;
                floorGridVertex[vIndex + 19] = -lineWidht;
                floorGridVertex[vIndex + 20] = 0;

                floorGridVertex[vIndex + 21] = 0f;
                floorGridVertex[vIndex + 22] = 0f;
                floorGridVertex[vIndex + 23] = 1f;
            }

            for (int i = 0; i < floorGridIndexes.Length; i += 1)
            {
                floorGridIndexes[i] = i;
            }

        }
    }
}
