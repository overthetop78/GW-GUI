namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFlicker
{
    internal static void Apply(float[] colors, long sequence, int intensity)
    {
        if (intensity <= 0 || (sequence & 1) == 0) return;
        var factor = 1f - Math.Clamp(intensity / 100f, 0f, 1f) * 0.5f;
        for (var index = 0; index < colors.Length; index++) colors[index] *= factor;
    }

    internal const string Shader = """
        vec3 filterFlicker(vec3 color,float sequence,float intensity)
        { return mod(sequence,2.0)>.5?color*(1.0-clamp(intensity,0.0,1.0)*0.5):color; }
        """;
}
