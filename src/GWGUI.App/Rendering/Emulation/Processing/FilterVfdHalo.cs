namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdHalo
{
    internal const string Shader = """
        float filterVfdHalo(float nearEmission,float farEmission,float intensity)
        {
            return clamp((nearEmission*.65+farEmission*.35)*intensity*.9,0.0,1.0);
        }
        """;

    internal static float[] Create(float[] emission, int width, int height, int sourceWidth,
        int sourceHeight, int radiusSetting, int intensitySetting)
    {
        var result = new float[emission.Length];
        if (intensitySetting <= 0) return result;
        var sourceRadius = FilterVfdHaloRadius.SourcePixels(radiusSetting);
        var radiusX = Math.Max(1, (int)MathF.Round(sourceRadius * width / sourceWidth));
        var radiusY = Math.Max(1, (int)MathF.Round(sourceRadius * height / sourceHeight));
        var nearX = Math.Max(1, radiusX / 2);
        var nearY = Math.Max(1, radiusY / 2);
        var intensity = intensitySetting / 100f * 0.9f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var near = Samples(emission, width, height, x, y, nearX, nearY);
            var far = Samples(emission, width, height, x, y, radiusX, radiusY);
            result[y * width + x] = Math.Clamp((near * 0.65f + far * 0.35f) * intensity, 0f, 1f);
        }
        return result;
    }

    private static float Samples(float[] values, int width, int height, int x, int y,
        int radiusX, int radiusY)
    {
        var sum = 0f;
        for (var offsetY = -1; offsetY <= 1; offsetY++)
        for (var offsetX = -1; offsetX <= 1; offsetX++)
        {
            if (offsetX == 0 && offsetY == 0) continue;
            var sampleX = Math.Clamp(x + offsetX * radiusX, 0, width - 1);
            var sampleY = Math.Clamp(y + offsetY * radiusY, 0, height - 1);
            sum += values[sampleY * width + sampleX];
        }
        return sum / 8f;
    }
}
