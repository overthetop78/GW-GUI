namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSharpBilinear
{
        internal static float Sample(float[] source, int width, int height,
        float x, float y, float scaleX, float scaleY, int channel) =>
        FilterBilinear.Sample(source, width, height, SharpCoordinate(x, scaleX), SharpCoordinate(y, scaleY), channel);

    private static float SharpCoordinate(float coordinate, float scale)
    {
        var floor = MathF.Floor(coordinate);
        var fraction = coordinate - floor;
        return floor + Math.Clamp((fraction - 0.5f) * scale + 0.5f, 0f, 1f);
    }
internal const string OpenGlShader = """
        vec4 sharpBilinearSample(vec2 uv)
        {
            vec2 pixel = uv * Processing.zw - 0.5;
            vec2 base = floor(pixel);
            vec2 fraction = pixel - base;
            vec2 scale = max(Output.xy / Processing.zw, vec2(0.0001));
            vec2 sharp = clamp((fraction - 0.5) * scale + 0.5, 0.0, 1.0);
            return linearSample((base + sharp + 0.5) / Processing.zw);
        }
        """;

    internal const string VeldridShader = """
        vec3 sharpBilinearSampleCompact(vec2 uv)
        {
            vec2 size=vec2(textureSize(sampler2D(Source,LinearSampler),0));
            vec2 pixel=uv*size-.5,base=floor(pixel),fraction=fract(pixel);
            vec2 scale=max(Parameters.Output.xy/size,vec2(1.0));
            vec2 sharp=clamp((fraction-.5)*scale*1.35+.5,0.0,1.0);
            vec2 mapped=(base+sharp+.5)/size;
            vec3 color=linearSampleCompact(mapped);
            vec2 stepSize=1.0/size;
            vec3 blur=(linearSampleCompact(mapped+vec2(stepSize.x,0.0))
                +linearSampleCompact(mapped-vec2(stepSize.x,0.0))
                +linearSampleCompact(mapped+vec2(0.0,stepSize.y))
                +linearSampleCompact(mapped-vec2(0.0,stepSize.y)))*.25;
            return clamp(color+(color-blur)*.28,0.0,1.0);
        }
        """;
}
