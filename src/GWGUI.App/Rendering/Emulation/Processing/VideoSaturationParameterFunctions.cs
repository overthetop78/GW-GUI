namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class VideoSaturationParameterFunctions
{
    private const float RedLuminance = 0.2126f;
    private const float GreenLuminance = 0.7152f;
    private const float BlueLuminance = 0.0722f;

    internal static void Apply(float[] colors, int setting)
    {
        if (setting == 0) return;
        var factor = 1f + setting / 10f;
        for (var index = 0; index < colors.Length; index += 3)
        {
            var luminance = colors[index] * RedLuminance
                + colors[index + 1] * GreenLuminance + colors[index + 2] * BlueLuminance;
            colors[index] = Math.Clamp(luminance + (colors[index] - luminance) * factor, 0f, 1f);
            colors[index + 1] = Math.Clamp(luminance + (colors[index + 1] - luminance) * factor, 0f, 1f);
            colors[index + 2] = Math.Clamp(luminance + (colors[index + 2] - luminance) * factor, 0f, 1f);
        }
    }

    internal const string Shader = """
        vec3 videoSaturationParameter(vec3 color,float factor)
        { float luminance=dot(color,vec3(0.2126,0.7152,0.0722)); return clamp(vec3(luminance)+(color-vec3(luminance))*factor,0.0,1.0); }
        """;}