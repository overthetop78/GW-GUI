using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixPalette
{
    internal const string Shader = "vec3 filterDotMatrixBackground(float palette){int p=int(palette+.5);if(p==0)return vec3(.16,.25,.075);if(p==1)return vec3(.64,.68,.62);return vec3(0.0);}vec3 filterDotMatrixForeground(vec3 source,float palette){int p=int(palette+.5);if(p==0)return vec3(.018,.045,.01);if(p==1)return vec3(.035);float y=max(dot(source,vec3(.2126,.7152,.0722)),.0001);if(p==4)return clamp(source/y,0.0,1.0);if(p==2)return vec3(1.0,.42,.015);if(p==3)return vec3(.32,.72,1.0);if(p==5)return vec3(1.0,.06,.025);return vec3(1.0);}";

    internal static ((float R, float G, float B) Background,
        (float R, float G, float B) Foreground) Apply(float red, float green, float blue,
        EmulationDotMatrixPalette palette)
    {
        var luminance = Math.Max(.0001f, red * .2126f + green * .7152f + blue * .0722f);
        return palette switch
        {
            EmulationDotMatrixPalette.Green => ((.16f, .25f, .075f), (.018f, .045f, .01f)),
            EmulationDotMatrixPalette.Gray => ((.64f, .68f, .62f), (.035f, .035f, .035f)),
            EmulationDotMatrixPalette.Amber => ((0f, 0f, 0f), (1f, .42f, .015f)),
            EmulationDotMatrixPalette.Blue => ((0f, 0f, 0f), (.32f, .72f, 1f)),
            EmulationDotMatrixPalette.Rgb => ((0f, 0f, 0f),
                (Math.Clamp(red / luminance, 0f, 1f),
                    Math.Clamp(green / luminance, 0f, 1f),
                    Math.Clamp(blue / luminance, 0f, 1f))),
            EmulationDotMatrixPalette.Red => ((0f, 0f, 0f), (1f, .06f, .025f)),
            _ => ((0f, 0f, 0f), (1f, 1f, 1f))
        };
    }
}
