namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixelResponse
{
    internal const string Shader = """
        vec3 filterFixedPixelResponse(vec3 previous,vec3 current,float responseMs,float elapsedMs)
        {
            float response=responseMs<=0.0?1.0:1.0-exp(-max(.001,elapsedMs)/responseMs);
            return mix(previous,current,response);
        }
        """;

    internal static float BlendFactor(int responseMilliseconds, double elapsedMilliseconds) =>
        responseMilliseconds <= 0 ? 1f : (float)(1d -
            Math.Exp(-Math.Max(0.001, elapsedMilliseconds) / responseMilliseconds));

    internal static float Apply(float previous, float current, float blendFactor) =>
        previous + (current - previous) * blendFactor;
}
