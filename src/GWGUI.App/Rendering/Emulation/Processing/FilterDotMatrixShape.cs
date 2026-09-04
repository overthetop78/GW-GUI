using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixShape
{
    internal const string Shader = "float filterDotMatrixDistance(vec2 p,float shape){return shape<.5?length(p):(shape<1.5?max(abs(p.x),abs(p.y)):max(abs(p.x)*.72,abs(p.y)));}";

    internal static float Distance(float x, float y, EmulationDotMatrixShape shape) => shape switch
    {
        EmulationDotMatrixShape.Square => MathF.Max(MathF.Abs(x), MathF.Abs(y)),
        EmulationDotMatrixShape.Rectangle => MathF.Max(MathF.Abs(x) * .72f, MathF.Abs(y)),
        _ => MathF.Sqrt(x * x + y * y)
    };
}
