using STL_Showcase.Logic.Rendering;
using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace STL_Showcase.Logic.Files
{
    public class ModelFileData
    {

        public Mesh3D Mesh { get; private set; }

        public Supported3DFiles FileType { get; private set; }
        private Stream FileBytes { get; set; }
        public long BytesLength { get { if (!this.HasBytes()) return 0; return FileBytes.Length; } }
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string FileFullPath { get; private set; }
        public float FileSizeKB { get; private set; }
        public float FileSizeMB { get; private set; }
        public string FileSizeString { get { return FileSizeKB < 1024f ? $"{FileSizeKB.ToString("0")} KB" : $"{FileSizeMB.ToString("0")} MB"; } }
        public DateTime DateModified { get; private set; }
        public DateTime DateCreated { get; private set; }
        public BitmapSource[] Thumnails { get; private set; }


        public ModelFileData() { }
        ~ModelFileData() { this.ReleaseData(true, true); }

        public ModelFileData(string FullPath)
        {
            this.SetFileData(FullPath);
        }

        public void SetFileData(string FullPath)
        {
            this.FileFullPath = FullPath;
            this.FilePath = System.IO.Path.GetDirectoryName(FullPath);
            this.FileName = System.IO.Path.GetFileName(FullPath);
        }
        public void SetThumnails(BitmapSource[] images)
        {
            this.Thumnails = images;
        }
        public bool HasThumbnails()
        {
            return this.Thumnails != null && this.Thumnails.Length > 0;
        }
        public Stream GetStream()
        {
            return this.FileBytes;
        }
        public bool HasBytes()
        {
            try
            {
                return this.FileBytes != null && this.FileBytes.Length > 100;
            }
            catch
            {
                return false;
            }
        }
        public bool ReadBytes(byte[] buffer, int start, int offset, int count)
        {
            try
            {
                if (!this.HasBytes() || this.FileBytes.Length < start + count)
                    return false;
                this.FileBytes.Position = start;
                this.FileBytes.Read(buffer, offset, count);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes the file bytes and loses the reference to the mesh data.
        /// </summary>
        public void ReleaseData(bool releaseFileData, bool releaseMeshData)
        {
            if (releaseFileData && this.FileBytes != null)
            {
                this.FileBytes.Dispose();
                this.FileBytes = null;
            }
            if (releaseMeshData)
                this.Mesh = null;
        }

        public bool LoadBasicFileData()
        {
            try
            {
                this.FileType = FileParser.CheckFileType(this);
                this.DateModified = File.GetLastWriteTime(this.FileFullPath);
                this.DateCreated = File.GetCreationTime(this.FileFullPath);
                long fileLenght = 0;
                if (this.HasBytes())
                    fileLenght = this.FileBytes.Length;
                else
                    using (var stream = File.OpenRead(this.FileFullPath))
                    {
                        fileLenght = stream.Length;
                    }
                this.FileSizeKB = fileLenght / 1024f;
                this.FileSizeMB = this.FileSizeKB / 1024f;
                return true;
            }
            catch
            {
                this.DateModified = DateTime.MinValue;
                this.DateCreated = DateTime.MinValue;
                return false;
            }
        }
        public bool LoadFileBytes(bool KeepInMemory)
        {
            try
            {
                if (FileBytes != null)
                    FileBytes.Dispose();

                FileStream stream = File.OpenRead(this.FileFullPath);

                if (KeepInMemory && stream.Length <= 100 * 1024 * 1024)
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    MemoryStream newMemoryStream = new MemoryStream();
                    newMemoryStream.Write(bytes, 0, bytes.Length);
                    newMemoryStream.Position = 0;
                    stream.Dispose();
                    this.FileBytes = newMemoryStream;
                }
                else
                {
                    this.FileBytes = stream;
                }
                return true;
            }
            catch
            {
                FileBytes = null;
                return false;
            }
        }
        /// <summary>
        /// Parses the loaded bytes. Returns true if success.
        /// </summary>
        public bool ParseFile()
        {
            this.FileType = FileParser.CheckFileType(this);

            if (this.FileType != Supported3DFiles.Unsuportted)
            {
                if (FileBytes == null) return false;
                this.Mesh = FileParser.ParseModelFile(this);
                if (this.Mesh != null)
                {
                    this.Mesh.CenterObject();
                    return true;
                }
            }
            return false;
        }

        public object OrderFunc(string field)
        {

            switch (field)
            {
                case "FileName":
                    return this.FileName;
                case "Directory":
                    return this.FileFullPath;
                case "DateModified":
                    return this.DateModified;
                case "DateCreated":
                    return this.DateCreated;
                case "FileSizeKB":
                    return this.FileSizeKB;
                default:
                    return "";
            }
        }

    }
}
