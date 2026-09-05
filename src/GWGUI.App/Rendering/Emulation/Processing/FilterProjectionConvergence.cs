namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionConvergence
{
    internal const string Shader = "float projectionConvergence(float setting){return setting*3.0;}";

    internal static float Apply(float setting) =>
        setting * 3f;
}

