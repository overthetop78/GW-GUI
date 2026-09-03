namespace GWGUI.App.Rendering.Emulation.Processing;

// Adapted from Sp00kyFox's MIT-licensed five-pass ScaleFX shader chain (2016).
// This portable pass retains its perceptual metric, 3x subpixel grid and source-palette output.
internal static class FilterScaleFx
{
    private const float CandidateThreshold = 0.5f;
    private const float DifferenceThreshold = 0.06f;

    public static void Sample(float[] source, int width, int height,
        float positionX, float positionY, Span<float> result)
    {
        var sourceX = Math.Clamp((int)MathF.Floor(positionX), 0, width - 1);
        var sourceY = Math.Clamp((int)MathF.Floor(positionY), 0, height - 1);
        var fractionX = positionX - MathF.Floor(positionX);
        var fractionY = positionY - MathF.Floor(positionY);
        var subpixelX = Math.Clamp((int)(fractionX * 3f), 0, 2);
        var subpixelY = Math.Clamp((int)(fractionY * 3f), 0, 2);

        var a = Read(sourceX - 1, sourceY - 1);
        var b = Read(sourceX, sourceY - 1);
        var c = Read(sourceX + 1, sourceY - 1);
        var d = Read(sourceX - 1, sourceY);
        var e = Read(sourceX, sourceY);
        var f = Read(sourceX + 1, sourceY);
        var g = Read(sourceX - 1, sourceY + 1);
        var h = Read(sourceX, sourceY + 1);
        var i = Read(sourceX + 1, sourceY + 1);

        var topLeft = Classify(e, d, b, a, h, f);
        var topRight = Classify(e, b, f, c, h, d);
        var bottomRight = Classify(e, f, h, i, d, b);
        var bottomLeft = Classify(e, h, d, g, b, f);

        var selected = (subpixelX, subpixelY) switch
        {
            (0, 0) => CornerOrCenter(topLeft, e),
            (1, 0) => SelectMid(e, b, topLeft, topRight),
            (2, 0) => CornerOrCenter(topRight, e),
            (0, 1) => SelectMid(e, d, topLeft, bottomLeft),
            (2, 1) => SelectMid(e, f, topRight, bottomRight),
            (0, 2) => CornerOrCenter(bottomLeft, e),
            (1, 2) => SelectMid(e, h, bottomLeft, bottomRight),
            (2, 2) => CornerOrCenter(bottomRight, e),
            _ => e
        };

        result[0] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(selected.Red);
        result[1] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(selected.Green);
        result[2] = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(selected.Blue);

        Color Read(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            var offset = (y * width + x) * 3;
            return new Color(
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset]),
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 1]),
                SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(source[offset + 2]));
        }
    }

    private static Corner Classify(Color center, Color horizontal, Color vertical,
        Color diagonal, Color oppositeHorizontal, Color oppositeVertical)
    {
        var junction = Distance(horizontal, vertical);
        var centerGap = MathF.Min(Distance(center, horizontal), Distance(center, vertical));
        if (junction >= CandidateThreshold || centerGap <= DifferenceThreshold
            || Distance(horizontal, oppositeHorizontal) <= DifferenceThreshold
            || Distance(vertical, oppositeVertical) <= DifferenceThreshold)
            return default;

        var continuation = MathF.Min(Distance(diagonal, horizontal), Distance(diagonal, vertical));
        var weight = Math.Clamp((CandidateThreshold - junction) / CandidateThreshold, 0f, 1f)
            * Math.Clamp((centerGap + continuation) / MathF.Max(junction, 0.001f), 0f, 1f);
        if (weight <= DifferenceThreshold) return default;

        var candidate = Distance(center, horizontal) <= Distance(center, vertical)
            ? horizontal : vertical;
        return new Corner(candidate, weight, true);
    }

    private static Color CornerOrCenter(Corner corner, Color center) =>
        corner.Active ? corner.Color : center;

    private static Color SelectMid(Color center, Color axial, Corner first, Corner second)
    {
        if (first.Active && second.Active && Distance(first.Color, second.Color) < CandidateThreshold)
            return Distance(axial, first.Color) <= Distance(axial, second.Color)
                ? first.Color : second.Color;
        var corner = first.Strength >= second.Strength ? first : second;
        return corner.Active && Distance(corner.Color, axial) < CandidateThreshold * 0.5f
            ? axial : center;
    }

    // Compuphase perceptual RGB metric used by ScaleFX pass 0.
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
    private readonly record struct Corner(Color Color, float Strength, bool Active);

    internal const string OpenGlShader = """
        vec4 scaleFxSample(vec2 uv)
        {
            vec2 position = uv * Processing.zw;
            vec2 pixel = floor(position);
            vec2 cell = floor(clamp(fract(position) * 3.0, 0.0, 2.0));
            vec3 up = xbrPointAt(pixel, vec2(0.0, -1.0));
            vec3 left = xbrPointAt(pixel, vec2(-1.0, 0.0));
            vec3 center = xbrPointAt(pixel, vec2(0.0));
            vec3 right = xbrPointAt(pixel, vec2(1.0, 0.0));
            vec3 down = xbrPointAt(pixel, vec2(0.0, 1.0));
            if (cell.x < 0.5 && cell.y < 0.5 && advancedSame(left, up)
                && !advancedSame(center, left)) return vec4(left, 1.0);
            if (cell.x > 1.5 && cell.y < 0.5 && advancedSame(up, right)
                && !advancedSame(center, right)) return vec4(right, 1.0);
            if (cell.x < 0.5 && cell.y > 1.5 && advancedSame(left, down)
                && !advancedSame(center, left)) return vec4(left, 1.0);
            if (cell.x > 1.5 && cell.y > 1.5 && advancedSame(down, right)
                && !advancedSame(center, right)) return vec4(right, 1.0);
            return vec4(center, 1.0);
        }
        """;

    internal const string VeldridShader = """
        vec3 scaleFxCompactSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 position=uv*vec2(size);ivec2 p=ivec2(floor(position));vec2 f=fract(position);
            vec3 c=filterPointAt(p),l=filterPointAt(p+ivec2(-1,0)),r=filterPointAt(p+ivec2(1,0));
            vec3 u=filterPointAt(p+ivec2(0,-1)),d=filterPointAt(p+ivec2(0,1));
            vec3 horizontal=f.x<.5?l:r,vertical=f.y<.5?u:d;
            float dh=filterColorDistance(c,horizontal),dv=filterColorDistance(c,vertical);
            vec3 candidate=dh<dv?horizontal:vertical;
            float corner=clamp((abs(f.x-.5)+abs(f.y-.5)-.28)*1.9,0.0,1.0);
            return mix(c,candidate,corner*clamp(1.0-min(dh,dv)*2.0,.18,.72));
        }
        """;
}
