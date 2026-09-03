namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalConnectionRgbScart
{
    internal const string Shader = """
        vec3 signalConnectionRgbScart(vec3 color,vec3 left,float amount)
        { return mix(color,left,amount*.14); }
        """;

    internal static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f * 0.14f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 3; channel++)
        {
            var center = (y * width + x) * 3 + channel;
            var neighbor = (y * width + Math.Max(0, x - 1)) * 3 + channel;
            colors[center] += (source[neighbor] - source[center]) * amount;
        }
    }
}
