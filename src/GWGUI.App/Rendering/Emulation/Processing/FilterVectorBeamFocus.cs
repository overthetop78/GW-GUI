namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorBeamFocus
{
    internal const string Shader = """
        float filterVectorBeamFocus(float center,float neighborhood,float focus)
        {
            return mix(neighborhood,center,.35+.65*focus);
        }
        """;

    internal static float[] Apply(float[] emission, int width, int height, int setting)
    {
        if (setting >= 100) return emission;
        var result = new float[emission.Length];
        var blur = (100 - setting) / 100f * 0.65f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var average = 0f;
            for (var offsetY = -1; offsetY <= 1; offsetY++)
            for (var offsetX = -1; offsetX <= 1; offsetX++)
                average += Sample(emission, width, height, x + offsetX, y + offsetY);
            var center = emission[y * width + x];
            result[y * width + x] = center + (average / 9f - center) * blur;
        }
        return result;
    }

    private static float Sample(float[] values, int width, int height, int x, int y) =>
        values[Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)];
}
