namespace GWGUI.App.Rendering.Emulation.Processing;

// Independent MIT-compatible implementation of the public Scale2x/Scale3x neighborhood rules.
// No source code from the GPL ScaleNx reference is used.
internal static class FilterScaleNx
{
    public static void Sample(float[] source, int width, int height,
        float positionX, float positionY, float scaleX, float scaleY, Span<float> result)
    {
        var x = Math.Clamp((int)MathF.Floor(positionX), 0, width - 1);
        var y = Math.Clamp((int)MathF.Floor(positionY), 0, height - 1);
        var fractionX = positionX - MathF.Floor(positionX);
        var fractionY = positionY - MathF.Floor(positionY);
        var b = Read(x, y - 1);
        var d = Read(x - 1, y);
        var e = Read(x, y);
        var f = Read(x + 1, y);
        var h = Read(x, y + 1);

        Color selected;
        if (b == h || d == f)
        {
            selected = e;
        }
        else if (MathF.Min(scaleX, scaleY) < 2.5f)
        {
            var cellX = Math.Clamp((int)(fractionX * 2f), 0, 1);
            var cellY = Math.Clamp((int)(fractionY * 2f), 0, 1);
            selected = (cellX, cellY) switch
            {
                (0, 0) when d == b => d,
                (1, 0) when b == f => f,
                (0, 1) when d == h => d,
                (1, 1) when h == f => f,
                _ => e
            };
        }
        else
        {
            var a = Read(x - 1, y - 1);
            var c = Read(x + 1, y - 1);
            var g = Read(x - 1, y + 1);
            var i = Read(x + 1, y + 1);
            var cellX = Math.Clamp((int)(fractionX * 3f), 0, 2);
            var cellY = Math.Clamp((int)(fractionY * 3f), 0, 2);
            selected = (cellX, cellY) switch
            {
                (0, 0) when d == b => d,
                (1, 0) when d == b && e != c || b == f && e != a => b,
                (2, 0) when b == f => f,
                (0, 1) when d == b && e != g || d == h && e != a => d,
                (2, 1) when b == f && e != i || h == f && e != c => f,
                (0, 2) when d == h => d,
                (1, 2) when d == h && e != i || h == f && e != g => h,
                (2, 2) when h == f => f,
                _ => e
            };
        }

        result[0] = selected.Red;
        result[1] = selected.Green;
        result[2] = selected.Blue;

        Color Read(int sourceX, int sourceY)
        {
            sourceX = Math.Clamp(sourceX, 0, width - 1);
            sourceY = Math.Clamp(sourceY, 0, height - 1);
            var offset = (sourceY * width + sourceX) * 3;
            return new Color(source[offset], source[offset + 1], source[offset + 2]);
        }
    }

    private readonly record struct Color(float Red, float Green, float Blue);

    internal const string OpenGlShader = """
        vec4 scaleNxSample(vec2 uv)
        {
            vec2 position = uv * Processing.zw;
            vec2 pixel = floor(position);
            vec2 cell = floor(clamp(fract(position) * 3.0, 0.0, 2.0));
            vec3 up = xbrPointAt(pixel, vec2(0.0, -1.0));
            vec3 left = xbrPointAt(pixel, vec2(-1.0, 0.0));
            vec3 center = xbrPointAt(pixel, vec2(0.0));
            vec3 right = xbrPointAt(pixel, vec2(1.0, 0.0));
            vec3 down = xbrPointAt(pixel, vec2(0.0, 1.0));
            if (advancedSame(up, down) || advancedSame(left, right))
                return vec4(center, 1.0);
            if (cell.x < 0.5 && cell.y < 0.5 && advancedSame(left, up))
                return vec4(left, 1.0);
            if (cell.x > 1.5 && cell.y < 0.5 && advancedSame(up, right))
                return vec4(right, 1.0);
            if (cell.x < 0.5 && cell.y > 1.5 && advancedSame(left, down))
                return vec4(left, 1.0);
            if (cell.x > 1.5 && cell.y > 1.5 && advancedSame(down, right))
                return vec4(right, 1.0);
            return vec4(center, 1.0);
        }
        """;

    internal const string VeldridShader = """
        bool filterSimilar(vec3 a,vec3 b){return filterColorDistance(a,b)<.035;}
        vec3 scaleNxCompactSample(vec2 uv)
        {
            ivec2 size=textureSize(sampler2D(Source,PointSampler),0);
            vec2 position=uv*vec2(size);ivec2 p=ivec2(floor(position));vec2 f=fract(position);
            vec3 c=filterPointAt(p),l=filterPointAt(p+ivec2(-1,0)),r=filterPointAt(p+ivec2(1,0));
            vec3 u=filterPointAt(p+ivec2(0,-1)),d=filterPointAt(p+ivec2(0,1));
            vec3 result=c;
            if(!filterSimilar(u,d)&&!filterSimilar(l,r))
            {
                if(f.x<.5&&f.y<.5&&filterSimilar(l,u))result=(l+u)*.5;
                else if(f.x>=.5&&f.y<.5&&filterSimilar(u,r))result=(u+r)*.5;
                else if(f.x<.5&&f.y>=.5&&filterSimilar(l,d))result=(l+d)*.5;
                else if(f.x>=.5&&f.y>=.5&&filterSimilar(d,r))result=(d+r)*.5;
            }
            return result;
        }
        """;
}
