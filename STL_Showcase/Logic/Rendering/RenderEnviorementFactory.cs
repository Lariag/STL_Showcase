using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Logic.Rendering
{
    class RenderEnviorementFactory
    {

        public enum EvnType
        {
            Viewport3D // Yes its a wpf viewport pls dont hate me.
            , OpenTK
        }

        public static IRenderEnviorement CreateEnviorement(EvnType t, int renderResolution)
        {
            switch (t)
            {
                case EvnType.Viewport3D:
                    return new RenderEnv_ViewPort3D(renderResolution);
                case EvnType.OpenTK:
                    return new RenderEnv_OpenTK(renderResolution);
                default:
                    throw new NotImplementedException($"Rendering Enviorement for {t.ToString()} is not implemented.");
            }
        }
    }
}
