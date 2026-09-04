namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaPhosphorIntensity
{
    internal const string Shader = """
        vec3 filterPlasmaPhosphorIntensity(vec3 color,float intensity)
        {
            if(intensity<=0.0)return color;
            float luminance=dot(color,vec3(.2126,.7152,.0722));
            vec3 saturated=mix(vec3(luminance),color,1.0+intensity*.28);
            vec3 emission=max(saturated-vec3(.42),vec3(0.0))*intensity*.48;
            return clamp(saturated+emission,0.0,1.0);
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
            for (var channel = 0; channel < 3; channel++)
            {
                var saturated = luminance + (colors[index + channel] - luminance)
                    * (1f + intensity * 0.28f);
                var emission = MathF.Max(0f, saturated - 0.42f) * intensity * 0.48f;
                colors[index + channel] = Math.Clamp(saturated + emission, 0f, 1f);
            }
        }
    }
}
