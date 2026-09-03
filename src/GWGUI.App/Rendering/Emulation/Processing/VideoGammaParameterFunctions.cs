using GWGUI.Emulation.Functions;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class VideoGammaParameterFunctions
{
    internal static void Apply(float[] colors, int setting)
    {
        if (setting == 0) return;
        var exponent = (float)EmulationImageAdjustmentFunctions.GammaExponent(setting);
        for (var index = 0; index < colors.Length; index++)
            colors[index] = Math.Clamp(MathF.Pow(colors[index], exponent), 0f, 1f);
    }

    internal const string Shader = """
        vec3 videoGammaParameter(vec3 color,float exponent)
        { return clamp(pow(color,vec3(exponent)),0.0,1.0); }
        """;}