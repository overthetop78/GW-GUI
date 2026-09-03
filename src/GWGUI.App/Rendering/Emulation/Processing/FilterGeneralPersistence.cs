namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class FilterGeneralPersistence
{
    private float[]? _history;
    private int _width;
    private int _height;
    private long _sequence;

    internal void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        var amount = Math.Clamp(intensity / 100f, 0f, 1f);
        if (amount <= 0f)
        {
            Reset();
            return;
        }
        var compatible = _history is not null && _width == width && _height == height
            && sequence > _sequence;
        if (compatible)
            for (var index = 0; index < colors.Length; index++)
                colors[index] = Math.Clamp(Math.Max(colors[index], _history![index] * amount), 0f, 1f);
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

    internal const string Shader = """
        vec3 filterGeneralPersistence(vec3 currentColor,vec3 previousColor,float amount)
        { return max(currentColor,previousColor*clamp(amount,0.0,1.0)); }
        """;
}
