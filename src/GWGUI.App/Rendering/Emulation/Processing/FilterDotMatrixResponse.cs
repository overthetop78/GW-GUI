namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixResponse
{
    internal const string Shader =
        "vec3 filterDotMatrixResponse(vec3 previous,vec3 current,float factor){return mix(previous,current,clamp(factor,0.0,1.0));}";

    internal static float BlendFactor(int milliseconds, double elapsedMilliseconds) =>
        milliseconds <= 0 ? 1f : (float)(1d - Math.Exp(-elapsedMilliseconds / milliseconds));
}
