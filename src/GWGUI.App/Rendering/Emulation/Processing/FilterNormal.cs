namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterNormal
{
        internal static float Sample(float[] source, int width, int height,
        float x, float y, int channel)
    {
        var sampleX = Math.Clamp((int)MathF.Round(x), 0, width - 1);
        var sampleY = Math.Clamp((int)MathF.Round(y), 0, height - 1);
        return source[(sampleY * width + sampleX) * 3 + channel];
    }
internal const string OpenGlShader = """
        vec4 pointSample(vec2 uv)
        {
            vec2 coordinate = clamp(floor(uv * Processing.zw), vec2(0.0), Processing.zw - 1.0);
            return texture2D(Source, (coordinate + 0.5) / Processing.zw);
        }
        """;

    internal const string VeldridShader = """
        vec3 filterPointAt(ivec2 coordinate)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            return texelFetch(sampler2D(Source,PointSampler),
                clamp(coordinate,ivec2(0),size-1),0).rgb;
        }
        vec3 nearestSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            return filterPointAt(ivec2(floor(uv*vec2(size))));
        }
        """;
}
