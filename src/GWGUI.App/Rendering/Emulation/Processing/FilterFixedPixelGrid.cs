namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixelGrid
{
    internal const string Shader = """
        vec3 filterFixedPixelGrid(vec3 color,vec2 fraction,float intensity,float gap,vec2 pixelScale)
        {
            if(intensity<=0.0||gap<=0.0)return color;
            vec2 scale=max(pixelScale,vec2(1.0));
            float halfGap=gap*.16;
            vec2 edge=min(fraction,vec2(1.0)-fraction);
            vec2 coverage=clamp((vec2(halfGap)+.5/scale-edge)*scale,vec2(0.0),vec2(1.0));
            float grid=1.0-(1.0-coverage.x)*(1.0-coverage.y);
            return color*(1.0-intensity*.75*grid);
        }
        """;

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, int intensitySetting, int gapSetting)
    {
        if (intensitySetting == 0 || gapSetting == 0 || sourceWidth <= 0 || sourceHeight <= 0)
            return;

        var intensity = intensitySetting / 100f;
        var halfGap = gapSetting / 100f * 0.16f;
        var pixelScaleX = MathF.Max(1f, outputWidth / (float)sourceWidth);
        var pixelScaleY = MathF.Max(1f, outputHeight / (float)sourceHeight);
        var sourcePerOutputX = sourceWidth / (float)outputWidth;
        var sourcePerOutputY = sourceHeight / (float)outputHeight;
        for (var y = 0; y < outputHeight; y++)
        {
            var fy = Fraction((y + 0.5f) * sourcePerOutputY);
            var coverageY = Coverage(fy, halfGap, pixelScaleY);
            for (var x = 0; x < outputWidth; x++)
            {
                var fx = Fraction((x + 0.5f) * sourcePerOutputX);
                var coverageX = Coverage(fx, halfGap, pixelScaleX);
                var coverage = 1f - (1f - coverageX) * (1f - coverageY);
                var factor = 1f - intensity * 0.75f * coverage;
                var index = (y * outputWidth + x) * 3;
                colors[index] *= factor;
                colors[index + 1] *= factor;
                colors[index + 2] *= factor;
            }
        }
    }

    private static float Coverage(float fraction, float halfGap, float pixelScale)
    {
        var edge = MathF.Min(fraction, 1f - fraction);
        return Math.Clamp((halfGap + 0.5f / pixelScale - edge) * pixelScale, 0f, 1f);
    }

    private static float Fraction(float value) => value - MathF.Floor(value);
}