using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STL_Showcase.Shared.Enums;
using System.Windows.Media.Imaging;

namespace STL_Showcase.Logic.Rendering
{
    /// <summary>
    /// Provides methods to setup a render enviorement and render a single object at a time.
    /// </summary>
    public interface IRenderEnviorement
    {
        void SetEnviorementOptions(RenderAspectEnum renderType);
        void SetModel(Mesh3D mesh);
        void RemoveModel();
        BitmapSource RenderImage();
    }
}
