namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaCellStructure
{
    internal const string Shader = """
        vec3 filterPlasmaCellStructure(vec3 color,vec2 fraction,float intensity,vec2 pixelScale)
        {
            if(intensity<=0.0)return color;
            vec2 scale=max(pixelScale,vec2(1.0));
            float resolvable=smoothstep(1.0,2.5,min(scale.x,scale.y));
            float subpixels=smoothstep(2.4,4.0,scale.x);
            int selected=int(floor(fraction.x*3.0));
            if(selected>2)selected=2;
            for(int channel=0;channel<3;channel++)
                if(channel!=selected)color[channel]*=1.0-intensity*.38*subpixels;
            vec2 distancePixels=min(fraction,vec2(1.0)-fraction)*scale;
            vec2 edge=vec2(1.0)-smoothstep(vec2(.08),vec2(.65),distancePixels);
            float border=1.0-(1.0-edge.x)*(1.0-edge.y);
            float borderStrength=intensity*mix(.22,.62,resolvable);
            float centerLift=1.0+intensity*.06*(1.0-border);
            return color*(1.0-borderStrength*border)*centerLift;
        }
        """;

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, int setting)
    {
        if (setting <= 0 || sourceWidth <= 0 || sourceHeight <= 0) return;
        var intensity = setting / 100f;
        var pixelScaleX = outputWidth / (float)sourceWidth;
        var pixelScaleY = outputHeight / (float)sourceHeight;
        var scaleX = MathF.Max(pixelScaleX, 1f);
        var scaleY = MathF.Max(pixelScaleY, 1f);
        var resolvable = SmoothStep(1f, 2.5f, MathF.Min(scaleX, scaleY));
        var subpixels = SmoothStep(2.4f, 4f, scaleX);
        var sourcePerOutputX = sourceWidth / (float)outputWidth;
        var sourcePerOutputY = sourceHeight / (float)outputHeight;
        for (var y = 0; y < outputHeight; y++)
        {
            var fy = Fraction((y + 0.5f) * sourcePerOutputY);
            var edgeY = Edge(fy, scaleY);
            for (var x = 0; x < outputWidth; x++)
            {
                var fx = Fraction((x + 0.5f) * sourcePerOutputX);
                var edgeX = Edge(fx, scaleX);
                var selected = Math.Min(2, (int)(fx * 3f));
                var border = 1f - (1f - edgeX) * (1f - edgeY);
                var borderStrength = intensity * (0.22f + (0.62f - 0.22f) * resolvable);
                var factor = (1f - borderStrength * border)
                    * (1f + intensity * 0.06f * (1f - border));
                var index = (y * outputWidth + x) * 3;
                for (var channel = 0; channel < 3; channel++)
                {
                    if (channel != selected)
                        colors[index + channel] *= 1f - intensity * 0.38f * subpixels;
                    colors[index + channel] *= factor;
                }
            }
        }
    }

    private static float Edge(float fraction, float scale) =>
        1f - SmoothStep(0.08f, 0.65f,
            MathF.Min(fraction, 1f - fraction) * scale);

    private static float Fraction(float value) => value - MathF.Floor(value);
    private static float SmoothStep(float low, float high, float value)
    {
        var position = Math.Clamp((value - low) / (high - low), 0f, 1f);
        return position * position * (3f - 2f * position);
    }
}
