namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class FilterInterlacing
{
    private float[]? _history;
    private int _width;
    private int _height;
    private long _sequence;

    internal void ApplyFieldWeave(float[] colors, int sourceWidth, int sourceHeight,
        long sequence, bool enabled, int visibility)
    {
        if (!enabled || visibility <= 0)
        {
            Reset();
            return;
        }
        var amount = Math.Clamp(visibility / 100f, 0f, 1f);
        var compatible = _history is not null && _width == sourceWidth && _height == sourceHeight
            && sequence > _sequence;
        var current = colors.ToArray();
        var inactiveParity = (int)(sequence & 1);
        if (compatible)
        {
            var fieldBrightness = 1f - amount * 0.35f;
            for (var sourceY = inactiveParity; sourceY < sourceHeight; sourceY += 2)
            for (var sourceX = 0; sourceX < sourceWidth; sourceX++)
            {
                var offset = (sourceY * sourceWidth + sourceX) * 3;
                colors[offset] = (colors[offset] * (1f - amount) + _history![offset] * amount)
                    * fieldBrightness;
                colors[offset + 1] = (colors[offset + 1] * (1f - amount) + _history[offset + 1] * amount)
                    * fieldBrightness;
                colors[offset + 2] = (colors[offset + 2] * (1f - amount) + _history[offset + 2] * amount)
                    * fieldBrightness;
            }
        }
        _history = current;
        _width = sourceWidth;
        _height = sourceHeight;
        _sequence = sequence;
    }

    internal void Reset()
    {
        _history = null;
        _width = 0;
        _height = 0;
        _sequence = 0;
    }

    internal const string Shader = """
        vec3 filterInterlacing(vec3 color,vec3 previousColor,vec2 uv,float sourceHeight,float sequence,float enabled,float visibility,float hasHistory)
        {
            if(enabled<0.5||visibility<=0.0||hasHistory<0.5)return color;
            float sourceLine=floor(clamp(uv.y,0.0,0.999999)*max(sourceHeight,1.0));
            return mod(sourceLine,2.0)==mod(sequence,2.0)
                ?mix(color,previousColor,clamp(visibility,0.0,1.0))
                    *(1.0-clamp(visibility,0.0,1.0)*0.35):color;
        }
        """;
}
