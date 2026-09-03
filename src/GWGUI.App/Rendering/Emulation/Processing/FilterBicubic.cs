namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterBicubic
{
        internal static float Sample(float[] source, int width, int height, float x, float y, int channel)
    {
        var left = (int)MathF.Floor(x);
        var top = (int)MathF.Floor(y);
        var sum = 0f;
        var weightSum = 0f;
        for (var offsetY = -1; offsetY <= 2; offsetY++)
        {
            var weightY = CubicWeight(y - (top + offsetY));
            for (var offsetX = -1; offsetX <= 2; offsetX++)
            {
                var weight = weightY * CubicWeight(x - (left + offsetX));
                sum += Read(source, width, height, left + offsetX, top + offsetY, channel) * weight;
                weightSum += weight;
            }
        }
        return Math.Clamp(weightSum == 0f ? 0f : sum / weightSum, 0f, 1f);
    }

    private static float CubicWeight(float distance)
    {
        const float coefficient = -0.5f;
        var value = MathF.Abs(distance);
        if (value <= 1f) return (coefficient + 2f) * value * value * value - (coefficient + 3f) * value * value + 1f;
        if (value < 2f) return coefficient * value * value * value - 5f * coefficient * value * value + 8f * coefficient * value - 4f * coefficient;
        return 0f;
    }

    private static float Read(float[] source, int width, int height, int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return source[(y * width + x) * 3 + channel];
    }
internal const string OpenGlShader = """
        float cubicWeight(float distance)
        {
            const float a = -0.5;
            float v = abs(distance);
            if (v <= 1.0) return (a + 2.0) * v * v * v - (a + 3.0) * v * v + 1.0;
            if (v < 2.0) return a * v * v * v - 5.0 * a * v * v + 8.0 * a * v - 4.0 * a;
            return 0.0;
        }

        vec4 bicubicSample(vec2 uv)
        {
            vec2 pixel = uv * Processing.zw - 0.5;
            vec2 base = floor(pixel);
            vec4 sum = vec4(0.0);
            float weightSum = 0.0;
            for (int y = -1; y <= 2; y++)
            {
                float wy = cubicWeight(pixel.y - (base.y + float(y)));
                for (int x = -1; x <= 2; x++)
                {
                    float weight = wy * cubicWeight(pixel.x - (base.x + float(x)));
                    vec2 coordinate = clamp(base + vec2(float(x), float(y)),
                        vec2(0.0), Processing.zw - 1.0);
                    sum += texture2D(Source, (coordinate + 0.5) / Processing.zw) * weight;
                    weightSum += weight;
                }
            }
            return clamp(sum / max(weightSum, 0.0001), 0.0, 1.0);
        }
        """;

    internal const string VeldridShader = """
        float filterCubicWeight(float distance)
        {
            const float a=-.5;
            float value=abs(distance);
            if(value<=1.0)return (a+2.0)*value*value*value-(a+3.0)*value*value+1.0;
            if(value<2.0)return a*value*value*value-5.0*a*value*value+8.0*a*value-4.0*a;
            return 0.0;
        }
        vec3 filterNeighborhood(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            ivec2 p=ivec2(floor(uv*vec2(size)));
            return (filterPointAt(p+ivec2(-1,0))+filterPointAt(p+ivec2(1,0))
                +filterPointAt(p+ivec2(0,-1))+filterPointAt(p+ivec2(0,1)))*.25;
        }
        vec3 bicubicSampleCompact(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 pixel=uv*vec2(size)-.5;
            ivec2 base=ivec2(floor(pixel));
            vec3 sum=vec3(0.0);float weightSum=0.0;
            for(int y=-1;y<=2;y++)
            {
                float wy=filterCubicWeight(pixel.y-float(base.y+y));
                for(int x=-1;x<=2;x++)
                {
                    float weight=wy*filterCubicWeight(pixel.x-float(base.x+x));
                    sum+=filterPointAt(base+ivec2(x,y))*weight;weightSum+=weight;
                }
            }
            return clamp(sum/max(weightSum,.0001),0.0,1.0);
        }
        """;
}
