using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedMatrixColor
{
    internal const string Shader = """
        vec3 filterLedMatrixColor(vec3 source,float color)
        {
            if(color<.5)return source;
            float luminance=dot(source,vec3(.2126,.7152,.0722));
            if(color<1.5)return luminance*vec3(1.0,.03,.01);
            if(color<2.5)return luminance*vec3(.03,1.0,.08);
            if(color<3.5)return luminance*vec3(1.0,.42,.02);
            if(color<4.5)return luminance*vec3(.03,.25,1.0);
            return vec3(luminance);
        }
        """;

    internal static void Apply(float[] emission, EmulationLedMatrixColor color)
    {
        if (color == EmulationLedMatrixColor.Rgb) return;
        var tint = color switch
        {
            EmulationLedMatrixColor.Red => (1f, .03f, .01f),
            EmulationLedMatrixColor.Green => (.03f, 1f, .08f),
            EmulationLedMatrixColor.Amber => (1f, .42f, .02f),
            EmulationLedMatrixColor.Blue => (.03f, .25f, 1f),
            _ => (1f, 1f, 1f)
        };
        for (var index = 0; index < emission.Length; index += 3)
        {
            var luminance = emission[index] * .2126f + emission[index + 1] * .7152f
                + emission[index + 2] * .0722f;
            emission[index] = luminance * tint.Item1;
            emission[index + 1] = luminance * tint.Item2;
            emission[index + 2] = luminance * tint.Item3;
        }
    }
}
