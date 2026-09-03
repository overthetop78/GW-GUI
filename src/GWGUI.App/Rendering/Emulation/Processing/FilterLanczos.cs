namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLanczos
{
    internal static float Sample(float[] source, int width, int height, float x, float y, int channel)
    {
        var left = (int)MathF.Floor(x); var top = (int)MathF.Floor(y);
        var sum = 0f; var weightSum = 0f;
        for (var oy = -2; oy <= 3; oy++)
        for (var ox = -2; ox <= 3; ox++)
        {
            var dx = x - (left + ox); var dy = y - (top + oy);
            if (MathF.Abs(dx) >= 3f || MathF.Abs(dy) >= 3f) continue;
            var weight = FilterSampling.Sinc(dx) * FilterSampling.Sinc(dx / 3f)
                * FilterSampling.Sinc(dy) * FilterSampling.Sinc(dy / 3f);
            sum += FilterSampling.Read(source, width, height, left + ox, top + oy, channel) * weight;
            weightSum += weight;
        }
        return Math.Clamp(weightSum == 0f ? 0f : sum / weightSum, 0f, 1f);
    }

    internal const string OpenGlShader = """
        float lanczosSinc(float v){return abs(v)<.0001?1.0:sin(3.14159265*v)/(3.14159265*v);}
        vec4 lanczosSample(vec2 uv){vec2 p=uv*Processing.zw-.5,b=floor(p);vec4 s=vec4(0.0);float ws=0.0;for(int y=-2;y<=3;y++)for(int x=-2;x<=3;x++){vec2 q=b+vec2(float(x),float(y)),d=p-q;float w=lanczosSinc(d.x)*lanczosSinc(d.x/3.0)*lanczosSinc(d.y)*lanczosSinc(d.y/3.0);q=clamp(q,vec2(0.0),Processing.zw-1.0);s+=texture2D(Source,(q+.5)/Processing.zw)*w;ws+=w;}return clamp(s/max(ws,.0001),0.0,1.0);}
        """;

    internal const string VeldridShader = """
        float lanczosSinc(float v){return abs(v)<.0001?1.0:sin(3.14159265*v)/(3.14159265*v);}
        vec3 lanczosSampleCompact(vec2 uv){ivec2 size=textureSize(sampler2D(Source,PointSampler),0);vec2 p=uv*vec2(size)-.5;ivec2 b=ivec2(floor(p));vec3 s=vec3(0.0);float ws=0.0;for(int y=-2;y<=3;y++)for(int x=-2;x<=3;x++){ivec2 q=b+ivec2(x,y);vec2 d=p-vec2(q);float w=lanczosSinc(d.x)*lanczosSinc(d.x/3.0)*lanczosSinc(d.y)*lanczosSinc(d.y/3.0);s+=filterPointAt(q)*w;ws+=w;}return clamp(s/max(ws,.0001),0.0,1.0);}
        """;
}