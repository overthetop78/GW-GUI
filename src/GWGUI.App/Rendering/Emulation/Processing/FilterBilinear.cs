namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterBilinear
{
        internal static float Sample(float[] source, int width, int height,
        float x, float y, int channel)
    {
        var left = (int)MathF.Floor(x);
        var top = (int)MathF.Floor(y);
        var fractionX = x - left;
        var fractionY = y - top;
        var topValue = Lerp(Read(source, width, height, left, top, channel), Read(source, width, height, left + 1, top, channel), fractionX);
        var bottomValue = Lerp(Read(source, width, height, left, top + 1, channel), Read(source, width, height, left + 1, top + 1, channel), fractionX);
        return Lerp(topValue, bottomValue, fractionY);
    }

    private static float Read(float[] source, int width, int height, int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return source[(y * width + x) * 3 + channel];
    }

    private static float Lerp(float first, float second, float amount) => first + (second - first) * amount;
internal const string OpenGlShader = """
        vec4 linearSample(vec2 uv)
        {
            return texture2D(Source, clamp(uv, 0.5 / Processing.zw, 1.0 - 0.5 / Processing.zw));
        }
        """;

    internal const string VeldridShader = """
        vec3 filterLinearAt(ivec2 coordinate)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            return texelFetch(sampler2D(Source,PointSampler),
                clamp(coordinate,ivec2(0),size-1),0).rgb;
        }
        vec3 linearSampleCompact(vec2 uv)
        {
            vec2 size=vec2(textureSize(sampler2D(Source,PointSampler),0));
            vec2 pixel=uv*size-.5;
            ivec2 base=ivec2(floor(pixel));
            vec2 fraction=fract(pixel);
            vec3 top=mix(filterLinearAt(base),filterLinearAt(base+ivec2(1,0)),fraction.x);
            vec3 bottom=mix(filterLinearAt(base+ivec2(0,1)),filterLinearAt(base+ivec2(1,1)),fraction.x);
            return mix(top,bottom,fraction.y);
        }
        """;
}
