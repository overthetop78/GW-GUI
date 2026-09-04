namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class FilterPlasmaPersistence
{
    internal const string Shader = """
        vec3 filterPlasmaPersistence(vec3 current,vec3 previous,float intensity)
        {
            float retention=min(intensity,.94);
            return clamp(max(current,previous*retention),0.0,1.0);
        }
        """;

    private float[]? _history;
    private int _width;
    private int _height;
    private long _sequence;

    internal void Apply(float[] colors, int width, int height, long sequence, int setting)
    {
        var compatible = setting > 0 && _history is not null
            && _width == width && _height == height && sequence >= _sequence;
        if (compatible)
        {
            var intensity = setting / 100f;
            var retention = MathF.Min(intensity, 0.94f);
            var elapsedFrames = Math.Max(1L, sequence - _sequence);
            var decay = MathF.Pow(retention, Math.Min(elapsedFrames, 120L));
            for (var index = 0; index < colors.Length; index++)
                colors[index] = Math.Clamp(Math.Max(colors[index], _history![index] * decay), 0f, 1f);
        }
        _history = colors.ToArray();
        _width = width;
        _height = height;
        _sequence = sequence;
    }

    internal void Reset()
    {
        _history = null;
        _width = 0;
        _height = 0;
        _sequence = 0;
    }
}
