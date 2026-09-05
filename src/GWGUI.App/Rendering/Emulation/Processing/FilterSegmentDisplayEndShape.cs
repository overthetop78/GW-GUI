using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayEndShape
{
    internal const string Shader = """
        float filterSegmentDistance(vec2 point,vec4 line,float endShape)
        {
            vec2 direction=line.zw-line.xy;
            float lengthSquared=max(dot(direction,direction),.000001);
            float rawPosition=dot(point-line.xy,direction)/lengthSquared;
            float position=clamp(rawPosition,0.0,1.0);
            float perpendicular=length(point-(line.xy+position*direction));
            float sideDistance=length(point-(line.xy+rawPosition*direction));
            float extension=max(0.0,abs(rawPosition-.5)-.5)*sqrt(lengthSquared);
            if(endShape<.5)
                return max(sideDistance,(extension+sideDistance)*.70710678);
            if(endShape>1.5)
                return max(sideDistance,extension);
            return perpendicular;
        }
        """;

    internal static float Distance(float x, float y, SegmentDisplayElement line,
        EmulationSegmentEndShape shape)
    {
        if (line.IsPoint)
        {
            var pointX = x - line.StartX;
            var pointY = y - line.StartY;
            return MathF.Sqrt(pointX * pointX + pointY * pointY);
        }
        var dx = line.EndX - line.StartX;
        var dy = line.EndY - line.StartY;
        var lengthSquared = dx * dx + dy * dy;
        var rawPosition = ((x - line.StartX) * dx + (y - line.StartY) * dy) / lengthSquared;
        var position = Math.Clamp(rawPosition, 0f, 1f);
        var nearestX = line.StartX + position * dx;
        var nearestY = line.StartY + position * dy;
        var perpendicular = MathF.Sqrt((x - nearestX) * (x - nearestX)
            + (y - nearestY) * (y - nearestY));
        if (shape == EmulationSegmentEndShape.Rounded) return perpendicular;
        var rawNearestX = line.StartX + rawPosition * dx;
        var rawNearestY = line.StartY + rawPosition * dy;
        var sideDistance = MathF.Sqrt((x - rawNearestX) * (x - rawNearestX)
            + (y - rawNearestY) * (y - rawNearestY));
        var extension = MathF.Max(0f, MathF.Abs(rawPosition - .5f) - .5f)
            * MathF.Sqrt(lengthSquared);
        return shape == EmulationSegmentEndShape.Straight
            ? MathF.Max(sideDistance, extension)
            : MathF.Max(sideDistance, (extension + sideDistance) * .70710678f);
    }
}
