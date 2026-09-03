namespace GWGUI.App.Rendering.Emulation.Processing;

// Level-1 corner rule adapted from Hyllian's MIT-licensed xbr-lv3.glsl.
internal static class FilterXbr
{
    private const float YWeight = 48f;

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
        EvaluateCorner(1, 1, fractionX, fractionY);
        EvaluateCorner(1, -1, fractionX, fractionY);
        EvaluateCorner(-1, -1, fractionX, fractionY);
        EvaluateCorner(-1, 1, fractionX, fractionY);
        for (var channel = 0; channel < 3; channel++)
            result[channel] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(
                Lerp(center[channel], candidate[channel], bestBlend));

        void EvaluateCorner(int directionX, int directionY, float fx, float fy)
        {
            var f = Read(source, width, height, centerX + directionX, centerY);
            var h = Read(source, width, height, centerX, centerY + directionY);
            var e = Luma(center);
            var fl = Luma(f);
            var hl = Luma(h);
            if (MathF.Abs(e - fl) <= 0.000001f || MathF.Abs(e - hl) <= 0.000001f) return;

            var c = LumaAt(directionX, -directionY);
            var g = LumaAt(-directionX, directionY);
            var i = LumaAt(directionX, directionY);
            var h5 = LumaAt(0, 2 * directionY);
            var f4 = LumaAt(2 * directionX, 0);
            var d = LumaAt(-directionX, 0);
            var i5 = LumaAt(directionX, 2 * directionY);
            var i4 = LumaAt(2 * directionX, directionY);
            var b = LumaAt(0, -directionY);
            var edge = Distance(e, c, g, i, h5, f4, hl, fl)
                < Distance(hl, d, i5, fl, i4, b, e, i);
            if (!edge) return;
            var towardX = directionX > 0 ? fx : 1f - fx;
            var towardY = directionY > 0 ? fy : 1f - fy;
            var blend = SmoothStep(1.1f, 1.9f, towardX + towardY);
            if (blend <= bestBlend) return;
            bestBlend = blend;
            candidate = MathF.Abs(e - fl) <= MathF.Abs(e - hl) ? f : h;
        }

        float LumaAt(int offsetX, int offsetY) => Luma(Read(source, width, height,
            centerX + offsetX, centerY + offsetY));
    }

    private static FilterColor Read(float[] source, int width, int height, int x, int y)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        var offset = (y * width + x) * 3;
        return new FilterColor(
            SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset]),
            SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 1]),
            SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 2]));
    }

    private static float Luma(FilterColor color) => YWeight
        * (0.299f * color[0] + 0.587f * color[1] + 0.114f * color[2]);

    private static float Distance(float a, float b, float c, float d,
        float e, float f, float g, float h) => MathF.Abs(a - b) + MathF.Abs(a - c)
        + MathF.Abs(d - e) + MathF.Abs(d - f) + 4f * MathF.Abs(g - h);

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var t = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;

    internal const string OpenGlShader = """
        vec3 xbrPointAt(vec2 center, vec2 offset)
        {
            vec2 coordinate = clamp(center + offset, vec2(0.0), Processing.zw - 1.0);
            return texture2D(Source, (coordinate + 0.5) / Processing.zw).rgb;
        }

        float xbrLuma(vec3 color)
        {
            return 48.0 * dot(color, vec3(0.299, 0.587, 0.114));
        }

        float xbrDistance(float a, float b, float c, float d,
            float e, float f, float g, float h)
        {
            return abs(a - b) + abs(a - c) + abs(d - e) + abs(d - f)
                + 4.0 * abs(g - h);
        }

        void xbrSelect(float e, float f, float h, float c, float g, float i,
            float h5, float f4, float d, float i5, float i4, float b,
            vec3 fColor, vec3 hColor, float toward,
            inout float bestBlend, inout vec3 bestColor)
        {
            if (abs(e - f) <= 0.000001 || abs(e - h) <= 0.000001) return;
            if (xbrDistance(e, c, g, i, h5, f4, h, f)
                >= xbrDistance(h, d, i5, f, i4, b, e, i)) return;
            float blend = smoothstep(1.1, 1.9, toward);
            if (blend <= bestBlend) return;
            bestBlend = blend;
            bestColor = abs(e - f) <= abs(e - h) ? fColor : hColor;
        }

        vec4 xbrSample(vec2 uv)
        {
            vec2 position = uv * Processing.zw;
            vec2 center = floor(position);
            vec2 fraction = fract(position);
            vec3 A1 = xbrPointAt(center, vec2(-1.0, -2.0));
            vec3 B1 = xbrPointAt(center, vec2(0.0, -2.0));
            vec3 C1 = xbrPointAt(center, vec2(1.0, -2.0));
            vec3 A0 = xbrPointAt(center, vec2(-2.0, -1.0));
            vec3 A = xbrPointAt(center, vec2(-1.0, -1.0));
            vec3 B = xbrPointAt(center, vec2(0.0, -1.0));
            vec3 C = xbrPointAt(center, vec2(1.0, -1.0));
            vec3 C4 = xbrPointAt(center, vec2(2.0, -1.0));
            vec3 D0 = xbrPointAt(center, vec2(-2.0, 0.0));
            vec3 D = xbrPointAt(center, vec2(-1.0, 0.0));
            vec3 color = xbrPointAt(center, vec2(0.0));
            vec3 F = xbrPointAt(center, vec2(1.0, 0.0));
            vec3 F4 = xbrPointAt(center, vec2(2.0, 0.0));
            vec3 G0 = xbrPointAt(center, vec2(-2.0, 1.0));
            vec3 G = xbrPointAt(center, vec2(-1.0, 1.0));
            vec3 H = xbrPointAt(center, vec2(0.0, 1.0));
            vec3 I = xbrPointAt(center, vec2(1.0, 1.0));
            vec3 I4 = xbrPointAt(center, vec2(2.0, 1.0));
            vec3 G5 = xbrPointAt(center, vec2(-1.0, 2.0));
            vec3 H5 = xbrPointAt(center, vec2(0.0, 2.0));
            vec3 I5 = xbrPointAt(center, vec2(1.0, 2.0));
            float a1 = xbrLuma(A1), b1 = xbrLuma(B1), c1 = xbrLuma(C1);
            float a0 = xbrLuma(A0), a = xbrLuma(A), b = xbrLuma(B);
            float c = xbrLuma(C), c4 = xbrLuma(C4), d0 = xbrLuma(D0);
            float d = xbrLuma(D), e = xbrLuma(color), f = xbrLuma(F);
            float f4 = xbrLuma(F4), g0 = xbrLuma(G0), g = xbrLuma(G);
            float h = xbrLuma(H), i = xbrLuma(I), i4 = xbrLuma(I4);
            float g5 = xbrLuma(G5), h5 = xbrLuma(H5), i5 = xbrLuma(I5);
            vec3 candidate = color;
            float blend = 0.0;
            xbrSelect(e,f,h,c,g,i,h5,f4,d,i5,i4,b,F,H,
                fraction.x + fraction.y, blend, candidate);
            xbrSelect(e,f,b,i,a,c,b1,f4,d,c1,c4,h,F,B,
                fraction.x + 1.0 - fraction.y, blend, candidate);
            xbrSelect(e,d,b,g,c,a,b1,d0,f,a1,a0,h,D,B,
                2.0 - fraction.x - fraction.y, blend, candidate);
            xbrSelect(e,d,h,a,i,g,h5,d0,f,g5,g0,b,D,H,
                1.0 - fraction.x + fraction.y, blend, candidate);
            return vec4(mix(color, candidate, blend), 1.0);
        }
        """;

    internal const string VeldridShader = """
        float filterColorDistance(vec3 a,vec3 b){return dot(abs(a-b),vec3(.299,.587,.114));}
        vec3 xbrCompactSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 position=uv*vec2(size);ivec2 p=ivec2(floor(position));vec2 f=fract(position);
            vec3 c=filterPointAt(p),h=filterPointAt(p+ivec2(f.x<.5?-1:1,0));
            vec3 v=filterPointAt(p+ivec2(0,f.y<.5?-1:1));
            vec3 d=filterPointAt(p+ivec2(f.x<.5?-1:1,f.y<.5?-1:1));
            float corner=pow(clamp(1.0-2.0*length(f-vec2(f.x<.5?0.0:1.0,f.y<.5?0.0:1.0)),0.0,1.0),1.4);
            float diagonal=filterColorDistance(h,v)<filterColorDistance(c,d)?1.0:0.35;
            vec3 edge=filterColorDistance(c,h)<filterColorDistance(c,v)?h:v;
            return mix(c,mix(edge,(h+v+d)/3.0,.35),corner*.52*diagonal);
        }
        """;
}
