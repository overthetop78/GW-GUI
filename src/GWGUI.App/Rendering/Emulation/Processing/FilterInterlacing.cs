namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterInterlacing
{
    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        long sequence, bool enabled, int visibility)
    {
        if (!enabled || visibility <= 0) return;
        var factor = 1f - Math.Clamp(visibility / 100f, 0f, 1f);
        var inactiveParity = (int)(sequence & 1);
        for (var sourceY = inactiveParity; sourceY < sourceHeight; sourceY += 2)
        for (var sourceX = 0; sourceX < sourceWidth; sourceX++)
        {
            var offset = (sourceY * sourceWidth + sourceX) * 3;
            colors[offset] *= factor;
            colors[offset + 1] *= factor;
            colors[offset + 2] *= factor;
        }
    }

    internal const string Shader = """
        vec3 filterInterlacing(vec3 color,vec2 uv,float sourceHeight,float sequence,float enabled,float visibility)
        {
            if(enabled<0.5||visibility<=0.0)return color;
            float sourceLine=floor(clamp(uv.y,0.0,0.999999)*max(sourceHeight,1.0));
            return mod(sourceLine,2.0)==mod(sequence,2.0)
                ?color*(1.0-clamp(visibility,0.0,1.0)):color;
        }
        """;
}
