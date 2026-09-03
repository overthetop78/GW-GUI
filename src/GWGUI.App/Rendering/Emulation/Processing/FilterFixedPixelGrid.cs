namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixelGrid
{
    internal const string Shader = """
        vec3 filterFixedPixelGrid(vec3 color,vec2 fraction,float intensity,float gap)
        {
            if(intensity<=0.0||gap<=0.0)return color;
            float halfGap=.03+gap*.43;
            float edge=min(min(fraction.x,1.0-fraction.x),min(fraction.y,1.0-fraction.y));
            float softness=max(.025,halfGap*.22);
            float coverage=1.0-smoothstep(max(0.0,halfGap-softness),halfGap,edge);
            return color*(1.0-intensity*.92*coverage);
        }
        """;

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, int intensitySetting, int gapSetting)
    {
        if (intensitySetting == 0 || gapSetting == 0 || sourceWidth <= 0 || sourceHeight <= 0)
            return;

        var intensity = intensitySetting / 100f;
        var halfGap = 0.03f + gapSetting / 100f * 0.43f;
        var softness = MathF.Max(0.025f, halfGap * 0.22f);
        var scaleX = sourceWidth / (float)outputWidth;
        var scaleY = sourceHeight / (float)outputHeight;
        for (var y = 0; y < outputHeight; y++)
        {
            var fy = Fraction((y + 0.5f) * scaleY);
            for (var x = 0; x < outputWidth; x++)
            {
                var fx = Fraction((x + 0.5f) * scaleX);
                var edge = Math.Min(Math.Min(fx, 1f - fx), Math.Min(fy, 1f - fy));
                var coverage = 1f - SmoothStep(halfGap - softness, halfGap, edge);
                var factor = 1f - intensity * 0.92f * coverage;
                var index = (y * outputWidth + x) * 3;
                colors[index] *= factor;
                colors[index + 1] *= factor;
                colors[index + 2] *= factor;
            }
        }
    }

    private static float Fraction(float value) => value - MathF.Floor(value);

    private static float SmoothStep(float start, float end, float value)
    {
        var t = Math.Clamp((value - start) / MathF.Max(end - start, float.Epsilon), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
