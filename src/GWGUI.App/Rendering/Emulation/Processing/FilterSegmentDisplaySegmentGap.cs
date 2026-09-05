namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplaySegmentGap
{
    internal const string Shader = "vec4 filterSegmentGap(vec4 line,float gap){vec2 d=line.zw-line.xy;float amount=clamp(gap,0.0,1.0)*.12;return vec4(line.xy+d*amount,line.zw-d*amount);}";

    internal static SegmentDisplayElement Apply(SegmentDisplayElement value, int gap)
    {
        if (value.IsPoint) return value;
        var amount = Math.Clamp(gap, 0, 100) / 100f * .12f;
        var dx = value.EndX - value.StartX;
        var dy = value.EndY - value.StartY;
        return value with
        {
            StartX = value.StartX + dx * amount, StartY = value.StartY + dy * amount,
            EndX = value.EndX - dx * amount, EndY = value.EndY - dy * amount
        };
    }
}
