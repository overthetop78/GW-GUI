namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterBlackFrameInsertion
{
    internal static void Apply(float[] colors, long sequence, bool enabled)
    {
        if (enabled && (sequence & 1) != 0) Array.Clear(colors);
    }

    internal const string Shader = """
        vec3 filterBlackFrameInsertion(vec3 color,float sequence,float enabled)
        { return enabled>0.5&&mod(sequence,2.0)>.5?vec3(0.0):color; }
        """;
}
