namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaBlackDepth
{
    internal const string Shader = """
        vec3 filterPlasmaBlackDepth(vec3 color,float intensity)
        {
            if(intensity<=0.0)return color;
            float luminance=dot(color,vec3(.2126,.7152,.0722));
            float shadow=smoothstep(0.0,.24,luminance);
            return color*mix(1.0,mix(.28,1.0,shadow),intensity);
        }
        """;

    internal static void Apply(float[] colors, int setting)
    {
        if (setting <= 0) return;
        var intensity = setting / 100f;
        for (var index = 0; index < colors.Length; index += 3)
        {
            var luminance = colors[index] * 0.2126f + colors[index + 1] * 0.7152f
                + colors[index + 2] * 0.0722f;
            var shadow = SmoothStep(0f, 0.24f, luminance);
            var factor = 1f + (0.28f + 0.72f * shadow - 1f) * intensity;
            colors[index] *= factor;
            colors[index + 1] *= factor;
            colors[index + 2] *= factor;
        }
    }

    private static float SmoothStep(float low, float high, float value)
    {
        var position = Math.Clamp((value - low) / (high - low), 0f, 1f);
        return position * position * (3f - 2f * position);
    }
}
