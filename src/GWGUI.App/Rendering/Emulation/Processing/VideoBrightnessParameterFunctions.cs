namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class VideoBrightnessParameterFunctions
{
    internal static void Apply(float[] colors, int setting)
    {
        if (setting == 0) return;
        var adjustment = setting / 20f;
        for (var index = 0; index < colors.Length; index++)
            colors[index] = Math.Clamp(colors[index] + adjustment, 0f, 1f);
    }

    internal const string Shader = """
        vec3 videoBrightnessParameter(vec3 color,float adjustment)
        { return clamp(color+vec3(adjustment),0.0,1.0); }
        """;}