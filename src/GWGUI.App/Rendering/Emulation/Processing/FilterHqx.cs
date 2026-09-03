namespace GWGUI.App.Rendering.Emulation.Processing;

// Adapted from mGBA's MIT-licensed hq2x.fs (Lior Halphon, 2015-2023).
internal static class FilterHqx
{
    public static void Sample(float[] source, int width, int height,
        float positionX, float positionY, Span<float> result)
    {
        var x = (int)MathF.Floor(positionX);
        var y = (int)MathF.Floor(positionY);
        var dx = positionX - MathF.Floor(positionX) > 0.5f ? -1 : 1;
        var dy = positionY - MathF.Floor(positionY) > 0.5f ? -1 : 1;
        var w0 = Read(x - dx, y - dy); var w1 = Read(x, y - dy);
        var w2 = Read(x + dx, y - dy); var w3 = Read(x - dx, y);
        var w4 = Read(x, y); var w5 = Read(x + dx, y);
        var w6 = Read(x - dx, y + dy); var w7 = Read(x, y + dy);
        var w8 = Read(x + dx, y + dy);
        var pattern = 0;
        if (Different(w0, w4)) pattern |= 1;
        if (Different(w1, w4)) pattern |= 2;
        if (Different(w2, w4)) pattern |= 4;
        if (Different(w3, w4)) pattern |= 8;
        if (Different(w5, w4)) pattern |= 16;
        if (Different(w6, w4)) pattern |= 32;
        if (Different(w7, w4)) pattern |= 64;
        if (Different(w8, w4)) pattern |= 128;
        FilterColor color;
        if ((P(0xBF, 0x37) || P(0xDB, 0x13)) && Different(w1, w5)) color = Mix(w4, 3, w3, 1);
        else if ((P(0xDB, 0x49) || P(0xEF, 0x6D)) && Different(w7, w3)) color = Mix(w4, 3, w1, 1);
        else if ((P(0x0B, 0x0B) || P(0xFE, 0x4A) || P(0xFE, 0x1A)) && Different(w3, w1)) color = w4;
        else if ((P(0x6F,0x2A)||P(0x5B,0x0A)||P(0xBF,0x3A)||P(0xDF,0x5A)||P(0x9F,0x8A)||P(0xCF,0x8A)||P(0xEF,0x4E)||P(0x3F,0x0E)||P(0xFB,0x5A)||P(0xBB,0x8A)||P(0x7F,0x5A)||P(0xAF,0x8A)||P(0xEB,0x8A)) && Different(w3,w1)) color = Mix(w4,3,w0,1);
        else if (P(0x0B,0x08)) color = Mix(w4,2,w0,1,w1,1);
        else if (P(0x0B,0x02)) color = Mix(w4,2,w0,1,w3,1);
        else if (P(0x2F,0x2F)) color = Mix(w4,4,w3,1,w1,1);
        else if (P(0xBF,0x37)||P(0xDB,0x13)) color = Mix(w4,5,w1,2,w3,1);
        else if (P(0xDB,0x49)||P(0xEF,0x6D)) color = Mix(w4,5,w3,2,w1,1);
        else if (P(0x1B,0x03)||P(0x4F,0x43)||P(0x8B,0x83)||P(0x6B,0x43)) color = Mix(w4,3,w3,1);
        else if (P(0x4B,0x09)||P(0x8B,0x89)||P(0x1F,0x19)||P(0x3B,0x19)) color = Mix(w4,3,w1,1);
        else if (P(0x7E,0x2A)||P(0xEF,0xAB)||P(0xBF,0x8F)||P(0x7E,0x0E)) color = Mix(w4,2,w3,3,w1,3);
        else if (P(0xFB,0x6A)||P(0x6F,0x6E)||P(0x3F,0x3E)||P(0xFB,0xFA)||P(0xDF,0xDE)||P(0xDF,0x1E)) color = Mix(w4,3,w0,1);
        else if (P(0x0A,0x00)||P(0x4F,0x4B)||P(0x9F,0x1B)||P(0x2F,0x0B)||P(0xBE,0x0A)||P(0xEE,0x0A)||P(0x7E,0x0A)||P(0xEB,0x4B)||P(0x3B,0x1B)) color = Mix(w4,2,w3,1,w1,1);
        else color = Mix(w4,6,w3,1,w1,1);
        for (var channel = 0; channel < 3; channel++)
            result[channel] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(color[channel]);

        bool P(int mask, int expected) => (pattern & mask) == expected;
        FilterColor Read(int px, int py)
        {
            px = Math.Clamp(px, 0, width - 1); py = Math.Clamp(py, 0, height - 1);
            var offset = (py * width + px) * 3;
            return new FilterColor(
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset]),
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 1]),
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 2]));
        }
    }

    private static bool Different(FilterColor a, FilterColor b)
    {
        var r=a[0]-b[0]; var g=a[1]-b[1]; var bl=a[2]-b[2];
        return MathF.Abs(.25f*(r+g+bl))>.018f || MathF.Abs(.25f*(r-bl))>.002f
            || MathF.Abs(-.125f*r+.25f*g-.125f*bl)>.005f;
    }
    private static FilterColor Mix(FilterColor a,float aw,FilterColor b,float bw) =>
        new((a[0]*aw+b[0]*bw)/(aw+bw),(a[1]*aw+b[1]*bw)/(aw+bw),(a[2]*aw+b[2]*bw)/(aw+bw));
    private static FilterColor Mix(FilterColor a,float aw,FilterColor b,float bw,FilterColor c,float cw) =>
        new((a[0]*aw+b[0]*bw+c[0]*cw)/(aw+bw+cw),(a[1]*aw+b[1]*bw+c[1]*cw)/(aw+bw+cw),(a[2]*aw+b[2]*bw+c[2]*cw)/(aw+bw+cw));

    internal const string OpenGlShader = """
        vec3 hqxPointAt(vec2 center, vec2 offset)
        {
            vec2 coordinate = clamp(center + offset, vec2(0.0), Processing.zw - 1.0);
            return texture2D(Source, (coordinate + 0.5) / Processing.zw).rgb;
        }

        bool hqxDifferent(vec3 first, vec3 second)
        {
            vec3 difference = first - second;
            return abs(0.25 * (difference.r + difference.g + difference.b)) > 0.018
                || abs(0.25 * (difference.r - difference.b)) > 0.002
                || abs(-0.125 * difference.r + 0.25 * difference.g
                    - 0.125 * difference.b) > 0.005;
        }

        bool hqxPattern(float pattern, float mask, float expected)
        {
            float bit = 1.0;
            for (int index = 0; index < 8; index++)
            {
                float maskBit = mod(floor(mask / bit), 2.0);
                if (maskBit > 0.5)
                {
                    float actualBit = mod(floor(pattern / bit), 2.0);
                    float expectedBit = mod(floor(expected / bit), 2.0);
                    if (abs(actualBit - expectedBit) > 0.5) return false;
                }
                bit *= 2.0;
            }
            return true;
        }

        vec3 hqxMix(vec3 first, float firstWeight, vec3 second, float secondWeight)
        {
            return (first * firstWeight + second * secondWeight)
                / (firstWeight + secondWeight);
        }

        vec3 hqxMix(vec3 first, float firstWeight, vec3 second, float secondWeight,
            vec3 third, float thirdWeight)
        {
            return (first * firstWeight + second * secondWeight + third * thirdWeight)
                / (firstWeight + secondWeight + thirdWeight);
        }

        vec4 hqxSample(vec2 uv)
        {
            vec2 position = uv * Processing.zw;
            vec2 center = floor(position);
            vec2 direction = vec2(fract(position.x) > 0.5 ? -1.0 : 1.0,
                fract(position.y) > 0.5 ? -1.0 : 1.0);
            vec3 w0 = hqxPointAt(center, vec2(-direction.x, -direction.y));
            vec3 w1 = hqxPointAt(center, vec2(0.0, -direction.y));
            vec3 w2 = hqxPointAt(center, vec2(direction.x, -direction.y));
            vec3 w3 = hqxPointAt(center, vec2(-direction.x, 0.0));
            vec3 w4 = hqxPointAt(center, vec2(0.0));
            vec3 w5 = hqxPointAt(center, vec2(direction.x, 0.0));
            vec3 w6 = hqxPointAt(center, vec2(-direction.x, direction.y));
            vec3 w7 = hqxPointAt(center, vec2(0.0, direction.y));
            vec3 w8 = hqxPointAt(center, direction);
            float pattern = 0.0;
            if (hqxDifferent(w0, w4)) pattern += 1.0;
            if (hqxDifferent(w1, w4)) pattern += 2.0;
            if (hqxDifferent(w2, w4)) pattern += 4.0;
            if (hqxDifferent(w3, w4)) pattern += 8.0;
            if (hqxDifferent(w5, w4)) pattern += 16.0;
            if (hqxDifferent(w6, w4)) pattern += 32.0;
            if (hqxDifferent(w7, w4)) pattern += 64.0;
            if (hqxDifferent(w8, w4)) pattern += 128.0;
            vec3 color;
            if ((hqxPattern(pattern,191.0,55.0) || hqxPattern(pattern,219.0,19.0))
                && hqxDifferent(w1,w5)) color = hqxMix(w4,3.0,w3,1.0);
            else if ((hqxPattern(pattern,219.0,73.0) || hqxPattern(pattern,239.0,109.0))
                && hqxDifferent(w7,w3)) color = hqxMix(w4,3.0,w1,1.0);
            else if ((hqxPattern(pattern,11.0,11.0) || hqxPattern(pattern,254.0,74.0)
                || hqxPattern(pattern,254.0,26.0)) && hqxDifferent(w3,w1)) color = w4;
            else if ((hqxPattern(pattern,111.0,42.0) || hqxPattern(pattern,91.0,10.0)
                || hqxPattern(pattern,191.0,58.0) || hqxPattern(pattern,223.0,90.0)
                || hqxPattern(pattern,159.0,138.0) || hqxPattern(pattern,207.0,138.0)
                || hqxPattern(pattern,239.0,78.0) || hqxPattern(pattern,63.0,14.0)
                || hqxPattern(pattern,251.0,90.0) || hqxPattern(pattern,187.0,138.0)
                || hqxPattern(pattern,127.0,90.0) || hqxPattern(pattern,175.0,138.0)
                || hqxPattern(pattern,235.0,138.0)) && hqxDifferent(w3,w1))
                color = hqxMix(w4,3.0,w0,1.0);
            else if (hqxPattern(pattern,11.0,8.0)) color = hqxMix(w4,2.0,w0,1.0,w1,1.0);
            else if (hqxPattern(pattern,11.0,2.0)) color = hqxMix(w4,2.0,w0,1.0,w3,1.0);
            else if (hqxPattern(pattern,47.0,47.0)) color = hqxMix(w4,4.0,w3,1.0,w1,1.0);
            else if (hqxPattern(pattern,191.0,55.0) || hqxPattern(pattern,219.0,19.0))
                color = hqxMix(w4,5.0,w1,2.0,w3,1.0);
            else if (hqxPattern(pattern,219.0,73.0) || hqxPattern(pattern,239.0,109.0))
                color = hqxMix(w4,5.0,w3,2.0,w1,1.0);
            else if (hqxPattern(pattern,27.0,3.0) || hqxPattern(pattern,79.0,67.0)
                || hqxPattern(pattern,139.0,131.0) || hqxPattern(pattern,107.0,67.0))
                color = hqxMix(w4,3.0,w3,1.0);
            else if (hqxPattern(pattern,75.0,9.0) || hqxPattern(pattern,139.0,137.0)
                || hqxPattern(pattern,31.0,25.0) || hqxPattern(pattern,59.0,25.0))
                color = hqxMix(w4,3.0,w1,1.0);
            else if (hqxPattern(pattern,126.0,42.0) || hqxPattern(pattern,239.0,171.0)
                || hqxPattern(pattern,191.0,143.0) || hqxPattern(pattern,126.0,14.0))
                color = hqxMix(w4,2.0,w3,3.0,w1,3.0);
            else if (hqxPattern(pattern,251.0,106.0) || hqxPattern(pattern,111.0,110.0)
                || hqxPattern(pattern,63.0,62.0) || hqxPattern(pattern,251.0,250.0)
                || hqxPattern(pattern,223.0,222.0) || hqxPattern(pattern,223.0,30.0))
                color = hqxMix(w4,3.0,w0,1.0);
            else if (hqxPattern(pattern,10.0,0.0) || hqxPattern(pattern,79.0,75.0)
                || hqxPattern(pattern,159.0,27.0) || hqxPattern(pattern,47.0,11.0)
                || hqxPattern(pattern,190.0,10.0) || hqxPattern(pattern,238.0,10.0)
                || hqxPattern(pattern,126.0,10.0) || hqxPattern(pattern,235.0,75.0)
                || hqxPattern(pattern,59.0,27.0))
                color = hqxMix(w4,2.0,w3,1.0,w1,1.0);
            else color = hqxMix(w4,6.0,w3,1.0,w1,1.0);
            return vec4(color, 1.0);
        }
        """;

    internal const string VeldridShader = """
        vec3 hqxCompactSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 position=uv*vec2(size);ivec2 p=ivec2(floor(position));
            vec3 c=filterPointAt(p),l=filterPointAt(p+ivec2(-1,0)),r=filterPointAt(p+ivec2(1,0));
            vec3 u=filterPointAt(p+ivec2(0,-1)),d=filterPointAt(p+ivec2(0,1));
            float horizontal=filterColorDistance(l,r),vertical=filterColorDistance(u,d);
            vec3 direction=horizontal<vertical?(l+r)*.5:(u+d)*.5;
            float edge=clamp(min(horizontal,vertical)*3.0,0.0,1.0);
            vec3 highQuality=mix(linearSampleCompact(uv),direction,.32*(1.0-edge));
            return clamp(mix(highQuality,c,.18*edge),0.0,1.0);
        }
        """;
}
