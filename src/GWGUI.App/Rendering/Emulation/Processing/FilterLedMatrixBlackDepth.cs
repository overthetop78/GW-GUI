namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedMatrixBlackDepth
{
    internal const string Shader = """
        vec3 filterLedMatrixBlackDepth(float depth)
        {return vec3((1.0-clamp(depth,0.0,1.0))*.08);}
        """;

    internal static void Compose(float[] output, LedMatrixCellMap cells,
        int haloIntensity, int blackDepth)
    {
        var panel = (1f - Math.Clamp(blackDepth, 0, 100) / 100f) * .08f;
        var halo = FilterLedMatrixHalo.Intensity(haloIntensity);
        for (var pixel = 0; pixel < cells.CoreMask.Length; pixel++)
        {
            var light = cells.CoreMask[pixel] + cells.HaloMask[pixel] * halo;
            var color = pixel * 3;
            output[color] = Math.Clamp(panel + cells.Emission[color] * light, 0f, 1f);
            output[color + 1] = Math.Clamp(panel + cells.Emission[color + 1] * light, 0f, 1f);
            output[color + 2] = Math.Clamp(panel + cells.Emission[color + 2] * light, 0f, 1f);
        }
    }
}
