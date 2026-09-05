namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterEPaperDithering
{
    private static readonly int[,] Bayer4 = { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } };
    internal const string Shader = """
        float filterEPaperDither(vec2 pixel,float setting,float levels)
        {
            vec2 p=mod(floor(pixel),4.0);float x=p.x,y=p.y;
            float r0=x<.5?0.0:(x<1.5?8.0:(x<2.5?2.0:10.0));
            float r1=x<.5?12.0:(x<1.5?4.0:(x<2.5?14.0:6.0));
            float r2=x<.5?3.0:(x<1.5?11.0:(x<2.5?1.0:9.0));
            float r3=x<.5?15.0:(x<1.5?7.0:(x<2.5?13.0:5.0));
            float b=y<.5?r0:(y<1.5?r1:(y<2.5?r2:r3));
            return ((b+.5)/16.0-.5)*clamp(setting,0.0,1.0)/max(levels,1.0);
        }
        """;
    internal static float Offset(int x, int y, int setting, int levels) =>
        ((Bayer4[y & 3, x & 3] + .5f) / 16f - .5f) * Math.Clamp(setting, 0, 100) / 100f / Math.Max(1, levels);
}
