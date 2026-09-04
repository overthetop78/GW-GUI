namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdGlass
{
    internal const string Shader = """
        vec3 filterVfdGlass(vec3 source,float darkening)
        {
            float luminance=dot(source,vec3(.2126,.7152,.0722));
            vec3 smoke=vec3(.025,.055,.065)+luminance*vec3(.08,.12,.13);
            return mix(source*.28,smoke,darkening);
        }
        """;

    internal static void Apply(float[] colors, int setting)
    {
        var amount = setting / 100f;
        for (var index = 0; index < colors.Length; index += 3)
        {
            var luminance = colors[index] * 0.2126f + colors[index + 1] * 0.7152f
                + colors[index + 2] * 0.0722f;
            colors[index] = colors[index] * 0.28f * (1f - amount)
                + (0.025f + luminance * 0.08f) * amount;
            colors[index + 1] = colors[index + 1] * 0.28f * (1f - amount)
                + (0.055f + luminance * 0.12f) * amount;
            colors[index + 2] = colors[index + 2] * 0.28f * (1f - amount)
                + (0.065f + luminance * 0.13f) * amount;
        }
    }
}
