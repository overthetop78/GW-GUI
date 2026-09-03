namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSepia
{
    internal const string Shader = """
        vec3 filterSepia(vec3 color,float enabled)
        {
            vec3 sepia=vec3(dot(color,vec3(.393,.769,.189)),
                dot(color,vec3(.349,.686,.168)),dot(color,vec3(.272,.534,.131)));
            sepia*=vec3(1.08,.92,.68);
            return clamp(mix(color,sepia,enabled),0.0,1.0);
        }
        """;

    public static void Apply(float[] colors, bool enabled)
    {
        if (!enabled) return;
        for (var index = 0; index < colors.Length; index += 3)
        {
            var red = colors[index];
            var green = colors[index + 1];
            var blue = colors[index + 2];
            colors[index] = Math.Clamp((red * 0.393f + green * 0.769f + blue * 0.189f)
                * 1.08f, 0f, 1f);
            colors[index + 1] = Math.Clamp((red * 0.349f + green * 0.686f + blue * 0.168f)
                * 0.92f, 0f, 1f);
            colors[index + 2] = Math.Clamp((red * 0.272f + green * 0.534f + blue * 0.131f)
                * 0.68f, 0f, 1f);
        }
    }
}
