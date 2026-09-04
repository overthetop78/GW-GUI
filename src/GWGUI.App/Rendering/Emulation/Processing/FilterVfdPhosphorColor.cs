using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdPhosphorColor
{
    internal const string Shader = """
        vec3 filterVfdPhosphorColor(float emission,float mode)
        {
            vec3 tint=mode<.5?vec3(.18,.78,1.0):
                (mode<1.5?vec3(.12,1.0,.28):
                (mode<2.5?vec3(1.0,.48,.04):vec3(1.0,.08,.03)));
            return tint*emission;
        }
        """;

    internal static (float Red, float Green, float Blue) Tint(EmulationVfdColor color) => color switch
    {
        EmulationVfdColor.Green => (0.12f, 1f, 0.28f),
        EmulationVfdColor.Amber => (1f, 0.48f, 0.04f),
        EmulationVfdColor.Red => (1f, 0.08f, 0.03f),
        _ => (0.18f, 0.78f, 1f)
    };

    internal static void Apply(float[] colors, float[] emission, float[] halo,
        EmulationVfdColor color)
    {
        var tint = Tint(color);
        for (var pixel = 0; pixel < emission.Length; pixel++)
        {
            var index = pixel * 3;
            var light = Math.Clamp(emission[pixel] + halo[pixel], 0f, 1f);
            colors[index] = Math.Clamp(colors[index] + light * tint.Red, 0f, 1f);
            colors[index + 1] = Math.Clamp(colors[index + 1] + light * tint.Green, 0f, 1f);
            colors[index + 2] = Math.Clamp(colors[index + 2] + light * tint.Blue, 0f, 1f);
        }
    }
}
