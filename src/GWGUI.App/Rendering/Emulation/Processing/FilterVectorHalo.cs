namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorHalo
{
    internal const string Shader = """
        vec3 filterVectorHalo(vec3 color,float averageEmission,float lineIntensity,
            float haloIntensity)
        {
            return clamp(color+vec3(averageEmission*lineIntensity*haloIntensity*.5),
                0.0,1.0);
        }
        """;

    internal static void Apply(float[] colors, float[] emission, int width, int height,
        int lineIntensitySetting, int haloSetting, int radiusSetting)
    {
        if (haloSetting <= 0) return;
        var strength = lineIntensitySetting / 100f * haloSetting / 100f * 0.5f;
        var radius = FilterVectorHaloRadius.Pixels(radiusSetting);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var average = 0f;
            var samples = 0;
            for (var offsetY = -radius; offsetY <= radius; offsetY++)
            for (var offsetX = -radius; offsetX <= radius; offsetX++)
            {
                average += Sample(emission, width, height, x + offsetX, y + offsetY);
                samples++;
            }
            var light = average / samples * strength;
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(colors[index] + light, 0f, 1f);
            colors[index + 1] = Math.Clamp(colors[index + 1] + light, 0f, 1f);
            colors[index + 2] = Math.Clamp(colors[index + 2] + light, 0f, 1f);
        }
    }

    private static float Sample(float[] values, int width, int height, int x, int y) =>
        values[Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)];
}
