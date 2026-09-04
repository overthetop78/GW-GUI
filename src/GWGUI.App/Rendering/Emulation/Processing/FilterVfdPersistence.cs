namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class FilterVfdPersistence
{
    internal const string Shader = """
        vec3 filterVfdPersistence(vec3 current,vec3 previous,float durationMs,float elapsedMs)
        {
            if(durationMs<=0.0)return current;
            float retention=exp(-max(elapsedMs,0.001)/max(durationMs,1.0));
            return max(current,previous*retention);
        }
        """;

    private float[]? _history;
    private int _width;
    private int _height;
    private TimeSpan _timestamp;

    internal void Apply(float[] colors, int width, int height, TimeSpan timestamp,
        int durationMilliseconds)
    {
        var compatible = _history is not null && _width == width && _height == height
            && timestamp >= _timestamp;
        if (compatible && durationMilliseconds > 0)
        {
            var elapsed = Math.Max(0.001, (timestamp - _timestamp).TotalMilliseconds);
            var retention = Math.Exp(-elapsed / Math.Max(1, durationMilliseconds));
            for (var index = 0; index < colors.Length; index++)
                colors[index] = Math.Max(colors[index], (float)(_history![index] * retention));
        }
        _history = colors.ToArray();
        _width = width;
        _height = height;
        _timestamp = timestamp;
    }

    internal void Reset()
    {
        _history = null;
        _width = 0;
        _height = 0;
        _timestamp = TimeSpan.Zero;
    }
}
