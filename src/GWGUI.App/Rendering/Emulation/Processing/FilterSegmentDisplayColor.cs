using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayColor
{
    internal const string Shader = "vec3 filterSegmentColor(float color){if(color<.5)return vec3(1.0,.025,.01);if(color<1.5)return vec3(.04,1.0,.08);if(color<2.5)return vec3(1.0,.42,.015);if(color<3.5)return vec3(.03,.28,1.0);return vec3(1.0);}";

    internal static (float R, float G, float B) Apply(EmulationSegmentDisplayColor color) =>
        color switch
        {
            EmulationSegmentDisplayColor.Green => (.04f, 1f, .08f),
            EmulationSegmentDisplayColor.Amber => (1f, .42f, .015f),
            EmulationSegmentDisplayColor.Blue => (.03f, .28f, 1f),
            EmulationSegmentDisplayColor.White => (1f, 1f, 1f),
            _ => (1f, .025f, .01f)
        };
}
