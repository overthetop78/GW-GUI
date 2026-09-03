namespace GWGUI.App.Rendering.Emulation.Processing;

// Original edge-aware 3x3 implementation; no GPL xBRZ source code is used.
internal static class FilterXbrz
{
    public static void Sample(float[] source, int width, int height,
        float sourcePositionX, float sourcePositionY, Span<float> result)
    {
        var centerX = (int)MathF.Floor(sourcePositionX);
        var centerY = (int)MathF.Floor(sourcePositionY);
        var fractionX = sourcePositionX - MathF.Floor(sourcePositionX);
        var fractionY = sourcePositionY - MathF.Floor(sourcePositionY);
        var center = Read(source, width, height, centerX, centerY);
        var candidate = center;
        var bestBlend = 0f;
        EvaluateCorner(1, 1, fractionX + fractionY);
        EvaluateCorner(1, -1, fractionX + 1f - fractionY);
        EvaluateCorner(-1, -1, 2f - fractionX - fractionY);
        EvaluateCorner(-1, 1, 1f - fractionX + fractionY);
        for (var channel = 0; channel < 3; channel++)
            result[channel] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(
                Lerp(center[channel], candidate[channel], bestBlend));

        void EvaluateCorner(int directionX, int directionY, float toward)
        {
            var horizontal = At(directionX, 0);
            var vertical = At(0, directionY);
            var diagonal = At(directionX, directionY);
            var oppositeHorizontal = At(-directionX, 0);
            var oppositeVertical = At(0, -directionY);
            var outerHorizontal = At(directionX, -directionY);
            var outerVertical = At(-directionX, directionY);
            var centerHorizontal = ColorDistance(center, horizontal);
            var centerVertical = ColorDistance(center, vertical);
            if (centerHorizontal <= 0.01f || centerVertical <= 0.01f) return;
            var edge = ColorDistance(center, outerHorizontal)
                + ColorDistance(center, outerVertical)
                + ColorDistance(vertical, diagonal)
                + ColorDistance(horizontal, diagonal)
                + 4f * ColorDistance(vertical, horizontal);
            var alternative = ColorDistance(vertical, oppositeHorizontal)
                + ColorDistance(horizontal, oppositeVertical)
                + 4f * ColorDistance(center, diagonal);
            if (edge >= alternative) return;
            var strong = edge * 1.5f < alternative;
            var blend = strong ? SmoothStep(0.9f, 1.7f, toward)
                : SmoothStep(1.2f, 1.9f, toward);
            if (blend <= bestBlend) return;
            bestBlend = blend;
            candidate = centerHorizontal <= centerVertical ? horizontal : vertical;
        }

        FilterColor At(int offsetX, int offsetY) => Read(source, width, height,
            centerX + offsetX, centerY + offsetY);
    }

    private static FilterColor Read(float[] source, int width, int height, int x, int y)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        var offset = (y * width + x) * 3;
        return new FilterColor(
            SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset]),
            SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 1]),
            SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 2])
        );
    }

    private static float ColorDistance(FilterColor first, FilterColor second)
    {
        var red = first[0] - second[0];
        var green = first[1] - second[1];
        var blue = first[2] - second[2];
        var y = 0.299f * red + 0.587f * green + 0.114f * blue;
        var cb = -0.168736f * red - 0.331264f * green + 0.5f * blue;
        var cr = 0.5f * red - 0.418688f * green - 0.081312f * blue;
        return 48f * MathF.Abs(y) + 7f * MathF.Abs(cb) + 6f * MathF.Abs(cr);
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var amount = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return amount * amount * (3f - 2f * amount);
    }

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;

    internal const string OpenGlShader = """
        float advancedColorDistance(vec3 first, vec3 second)
        {
            vec3 d = first - second;
            return 48.0 * abs(dot(d, vec3(0.299, 0.587, 0.114)))
                + 7.0 * abs(dot(d, vec3(-0.168736, -0.331264, 0.5)))
                + 6.0 * abs(dot(d, vec3(0.5, -0.418688, -0.081312)));
        }

        bool advancedSame(vec3 first, vec3 second)
        {
            return advancedColorDistance(first, second) < 0.55;
        }

        vec4 xbrzSample(vec2 uv)
        {
            vec3 center = pointSample(uv).rgb;
            vec3 filtered = xbrSample(uv).rgb;
            float edge = clamp(advancedColorDistance(center, filtered) / 8.0, 0.0, 1.0);
            return vec4(mix(filtered, center, 0.18 * (1.0 - edge)), 1.0);
        }
        """;

    internal const string VeldridShader = """
        vec3 xbrzCompactSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 position=uv*vec2(size);ivec2 p=ivec2(floor(position));vec2 f=fract(position);
            ivec2 direction=ivec2(f.x<.5?-1:1,f.y<.5?-1:1);
            vec3 c=filterPointAt(p),h=filterPointAt(p+ivec2(direction.x,0));
            vec3 v=filterPointAt(p+ivec2(0,direction.y)),d=filterPointAt(p+direction);
            vec3 h2=filterPointAt(p+ivec2(direction.x*2,0));
            vec3 v2=filterPointAt(p+ivec2(0,direction.y*2));
            vec2 cornerPosition=vec2(direction.x<0?0.0:1.0,direction.y<0?0.0:1.0);
            float corner=clamp(1.15-1.6*length(f-cornerPosition),0.0,1.0);
            float edgeAgreement=1.0-clamp(abs(filterColorDistance(c,h)-filterColorDistance(c,v))*2.5,0.0,1.0);
            vec3 smoothed=(c*2.0+h*2.0+v*2.0+d+h2+v2)/9.0;
            return mix(c,smoothed,corner*(.58+.22*edgeAgreement));
        }
        """;
}
