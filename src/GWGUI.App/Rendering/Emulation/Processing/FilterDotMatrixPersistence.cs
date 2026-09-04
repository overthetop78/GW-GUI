namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixPersistence
{
    internal const string Shader =
        "vec3 filterDotMatrixPersistence(vec3 current,vec3 previous,float decay,float palette,vec3 background){return palette<1.5?min(current,background-(background-previous)*decay):max(current,previous*decay);}";

    internal static float Apply(float current, float previous, int milliseconds,
        double elapsedMilliseconds, bool reflective, float background)
    {
        if (milliseconds <= 0) return current;
        var decay = MathF.Exp(-(float)elapsedMilliseconds / milliseconds);
        return reflective
            ? MathF.Min(current, background - (background - previous) * decay)
            : MathF.Max(current, previous * decay);
    }
}
