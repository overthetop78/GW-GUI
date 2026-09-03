namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class VideoContrastParameterFunctions
{
    private const float LinearPivot = 0.18f;

    internal static void Apply(float[] colors, int setting)
    {
        if (setting == 0) return;
        var factor = MathF.Pow(2f, setting / 5f);
        for (var index = 0; index < colors.Length; index++)
            colors[index] = Math.Clamp((colors[index] - LinearPivot) * factor + LinearPivot, 0f, 1f);
    }

    internal const string Shader = """
        vec3 videoContrastParameter(vec3 color,float factor)
        { return clamp((color-vec3(0.18))*factor+vec3(0.18),0.0,1.0); }
        """;}