using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorPhosphorColor
{
    internal const string Shader = """
        vec3 filterVectorPhosphorColor(vec3 color,float mode)
        {
            if(mode<.5)return color;
            float luminance=dot(color,vec3(.2126,.7152,.0722));
            vec3 tint=mode<1.5?vec3(.18,1.0,.24):
                (mode<2.5?vec3(1.0,.46,.035):
                (mode<3.5?vec3(1.0):vec3(.78,.80,.77)));
            return luminance*tint;
        }
        """;

    internal static void Apply(float[] colors, EmulationCrtColorMode mode)
    {
        if (mode == EmulationCrtColorMode.Color) return;
        var tint = mode switch
        {
            EmulationCrtColorMode.Green => (0.18f, 1f, 0.24f),
            EmulationCrtColorMode.Amber => (1f, 0.46f, 0.035f),
            EmulationCrtColorMode.White => (1f, 1f, 1f),
            _ => (0.78f, 0.80f, 0.77f)
        };
        for (var index = 0; index < colors.Length; index += 3)
        {
            var luminance = colors[index] * 0.2126f + colors[index + 1] * 0.7152f
                + colors[index + 2] * 0.0722f;
            colors[index] = luminance * tint.Item1;
            colors[index + 1] = luminance * tint.Item2;
            colors[index + 2] = luminance * tint.Item3;
        }
    }
}
