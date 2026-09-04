namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorBeamWidth
{
    internal const string Shader = """
        float filterVectorBeamWidth(float center,float nearEmission,float farEmission,float intensity)
        {
            float widened=mix(center,max(center,nearEmission),min(intensity*1.5,1.0));
            return mix(widened,max(widened,farEmission),max(intensity-.67,0.0)*3.03);
        }
        """;

    internal static float[] Apply(float[] emission, int width, int height, int setting)
    {
        if (setting <= 0) return emission;
        var result = new float[emission.Length];
        var amount = setting / 100f;
        var radius = 1 + (int)MathF.Floor(amount * 2.99f);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var maximum = emission[y * width + x];
            for (var offsetY = -radius; offsetY <= radius; offsetY++)
            for (var offsetX = -radius; offsetX <= radius; offsetX++)
                if (offsetX * offsetX + offsetY * offsetY <= radius * radius)
                    maximum = MathF.Max(maximum, Sample(emission, width, height,
                        x + offsetX, y + offsetY));
            result[y * width + x] = emission[y * width + x]
                + (maximum - emission[y * width + x]) * amount;
        }
        return result;
    }

    private static float Sample(float[] values, int width, int height, int x, int y) =>
        values[Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)];
}
