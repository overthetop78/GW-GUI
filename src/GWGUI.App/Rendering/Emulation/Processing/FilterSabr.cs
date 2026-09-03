namespace GWGUI.App.Rendering.Emulation.Processing;

// Original MIT-compatible diagonal reconstruction; no GPL SABR source code is used.
internal static class FilterSabr
{
    public static void Sample(float[] source, int width, int height,
        float positionX, float positionY, Span<float> result)
    {
        var x = Math.Clamp((int)MathF.Floor(positionX), 0, width - 1);
        var y = Math.Clamp((int)MathF.Floor(positionY), 0, height - 1);
        var fractionX = positionX - MathF.Floor(positionX);
        var fractionY = positionY - MathF.Floor(positionY);
        var a = Read(x - 1, y - 1);
        var b = Read(x, y - 1);
        var c = Read(x + 1, y - 1);
        var d = Read(x - 1, y);
        var e = Read(x, y);
        var f = Read(x + 1, y);
        var g = Read(x - 1, y + 1);
        var h = Read(x, y + 1);
        var i = Read(x + 1, y + 1);

        var best = default(Candidate);
        Consider(d, b, a, 1.25f - 1.5f * (fractionX + fractionY));
        Consider(b, f, c, 1.25f - 1.5f * ((1f - fractionX) + fractionY));
        Consider(f, h, i, 1.25f - 1.5f * ((1f - fractionX) + (1f - fractionY)));
        Consider(h, d, g, 1.25f - 1.5f * (fractionX + (1f - fractionY)));

        var selected = best.Strength > 0f ? Mix(e, best.Color, best.Strength) : e;
        result[0] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(selected.Red);
        result[1] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(selected.Green);
        result[2] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(selected.Blue);

        void Consider(Color horizontal, Color vertical, Color diagonal, float proximity)
        {
            proximity = Math.Clamp(proximity, 0f, 1f);
            if (proximity <= 0f) return;
            var junction = Distance(horizontal, vertical);
            var centerHorizontal = Distance(e, horizontal);
            var centerVertical = Distance(e, vertical);
            var contrast = Math.Clamp(MathF.Min(centerHorizontal, centerVertical) / 0.2f, 0f, 1f);
            var coherence = 1f - Math.Clamp(junction / 0.45f, 0f, 1f);
            var opposingEnergy = centerHorizontal + centerVertical + 0.001f;
            var junctionEnergy = junction + 0.5f * Distance(e, diagonal);
            var dominance = Math.Clamp((opposingEnergy - junctionEnergy) / opposingEnergy, 0f, 1f);
            var strength = 0.75f * proximity * coherence * contrast * (0.4f + 0.6f * dominance);
            if (strength <= best.Strength) return;
            best = new Candidate(Mix(horizontal, vertical, 0.5f), strength);
        }

        Color Read(int sourceX, int sourceY)
        {
            sourceX = Math.Clamp(sourceX, 0, width - 1);
            sourceY = Math.Clamp(sourceY, 0, height - 1);
            var offset = (sourceY * width + sourceX) * 3;
            return new Color(
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset]),
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 1]),
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 2]));
        }
    }

    private static Color Mix(Color first, Color second, float weight) => new(
        first.Red + (second.Red - first.Red) * weight,
        first.Green + (second.Green - first.Green) * weight,
        first.Blue + (second.Blue - first.Blue) * weight);

    private static float Distance(Color first, Color second)
    {
        var redMean = 0.5f * (first.Red + second.Red);
        var red = first.Red - second.Red;
        var green = first.Green - second.Green;
        var blue = first.Blue - second.Blue;
        return MathF.Sqrt((2f + redMean) * red * red + 4f * green * green
            + (3f - redMean) * blue * blue) / 3f;
    }

    private readonly record struct Color(float Red, float Green, float Blue);
    private readonly record struct Candidate(Color Color, float Strength);

    internal const string OpenGlShader = """
        vec4 sabrSample(vec2 uv)
        {
            vec2 position = uv * Processing.zw;
            vec2 pixel = floor(position);
            vec2 fraction = fract(position);
            vec3 center = xbrPointAt(pixel, vec2(0.0));
            vec3 horizontal = xbrPointAt(pixel,
                vec2(fraction.x < 0.5 ? -1.0 : 1.0, 0.0));
            vec3 vertical = xbrPointAt(pixel,
                vec2(0.0, fraction.y < 0.5 ? -1.0 : 1.0));
            float proximity = clamp(1.25 - 1.5
                * (abs(fraction.x - 0.5) + abs(fraction.y - 0.5)), 0.0, 1.0);
            float coherence = 1.0 - clamp(
                advancedColorDistance(horizontal, vertical) / 18.0, 0.0, 1.0);
            float contrast = clamp(min(advancedColorDistance(center, horizontal),
                advancedColorDistance(center, vertical)) / 8.0, 0.0, 1.0);
            return vec4(mix(center, (horizontal + vertical) * 0.5,
                0.78 * proximity * coherence * contrast), 1.0);
        }
        """;

    internal const string VeldridShader = """
        vec3 sabrCompactSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 position=uv*vec2(size);ivec2 p=ivec2(floor(position));vec2 f=fract(position);
            vec3 c=filterPointAt(p),l=filterPointAt(p+ivec2(-1,0)),r=filterPointAt(p+ivec2(1,0));
            vec3 u=filterPointAt(p+ivec2(0,-1)),d=filterPointAt(p+ivec2(0,1));
            vec3 nw=filterPointAt(p+ivec2(-1,-1)),ne=filterPointAt(p+ivec2(1,-1));
            vec3 sw=filterPointAt(p+ivec2(-1,1)),se=filterPointAt(p+ivec2(1,1));
            float slash=filterColorDistance(nw,se)+filterColorDistance(l,d)+filterColorDistance(u,r);
            float backslash=filterColorDistance(ne,sw)+filterColorDistance(r,d)+filterColorDistance(u,l);
            vec3 diagonal=slash<backslash?mix(nw,se,(f.x+f.y)*.5):mix(ne,sw,(1.0-f.x+f.y)*.5);
            float edge=clamp(abs(slash-backslash)*2.0,0.12,.68);
            return clamp(mix(linearSampleCompact(uv),diagonal,edge*.48),0.0,1.0);
        }
        """;
}
