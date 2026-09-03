namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterJinc2
{
    internal static float Sample(float[] source, int width, int height, float x, float y, int channel)
    {
        var left = (int)MathF.Floor(x);
        var top = (int)MathF.Floor(y);
        var sum = 0f; var weightSum = 0f;
        for (var oy = -1; oy <= 2; oy++)
        for (var ox = -1; ox <= 2; ox++)
        {
            var distance = MathF.Sqrt(MathF.Pow(x - (left + ox), 2) + MathF.Pow(y - (top + oy), 2));
            if (distance >= 2f) continue;
            var weight = FilterSampling.Jinc(MathF.PI * distance)
                * FilterSampling.Jinc(MathF.PI * distance * .5f);
            sum += FilterSampling.Read(source, width, height, left + ox, top + oy, channel) * weight;
            weightSum += weight;
        }
        return Math.Clamp(weightSum == 0f ? 0f : sum / weightSum, 0f, 1f);
    }

    internal const string OpenGlShader = """
        float jinc2J1(float v){float h=v*.5,t=h,s=t;for(int i=1;i<12;i++){t*=-(h*h)/(float(i)*float(i+1));s+=t;}return s;}
        float jinc2Kernel(float v){return abs(v)<.0001?.5:jinc2J1(v)/v;}
        vec4 jinc2Sample(vec2 uv){vec2 p=uv*Processing.zw-.5,b=floor(p);vec4 s=vec4(0.0);float ws=0.0;for(int y=-1;y<=2;y++)for(int x=-1;x<=2;x++){vec2 q=b+vec2(float(x),float(y));float d=length(p-q);if(d<2.0){float w=jinc2Kernel(3.14159265*d)*jinc2Kernel(1.57079633*d);q=clamp(q,vec2(0.0),Processing.zw-1.0);s+=texture2D(Source,(q+.5)/Processing.zw)*w;ws+=w;}}return clamp(s/max(ws,.0001),0.0,1.0);}
        """;

    internal const string VeldridShader = """
        float jinc2J1(float v){float h=v*.5,t=h,s=t;for(int i=1;i<12;i++){t*=-(h*h)/(float(i)*float(i+1));s+=t;}return s;}
        float jinc2Kernel(float v){return abs(v)<.0001?.5:jinc2J1(v)/v;}
        vec3 jinc2SampleCompact(vec2 uv){ivec2 size=textureSize(sampler2D(Source,PointSampler),0);vec2 p=uv*vec2(size)-.5;ivec2 b=ivec2(floor(p));vec3 s=vec3(0.0);float ws=0.0;for(int y=-1;y<=2;y++)for(int x=-1;x<=2;x++){ivec2 q=b+ivec2(x,y);float d=length(p-vec2(q));if(d<2.0){float w=jinc2Kernel(3.14159265*d)*jinc2Kernel(1.57079633*d);s+=filterPointAt(q)*w;ws+=w;}}return clamp(s/max(ws,.0001),0.0,1.0);}
        """;
}