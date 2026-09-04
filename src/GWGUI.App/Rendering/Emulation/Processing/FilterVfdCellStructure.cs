using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdCellStructure
{
    internal const string Shader = """
        float filterVfdCellStructure(vec2 uv,vec2 sourceSize,float structure,
            float cellSize,float cellGap)
        {
            if(structure<.5)return 1.0;
            vec2 local=fract(uv*sourceSize)-.5;
            float radius=mix(.20,.48,cellSize)*(1.0-cellGap*.72);
            float distanceToCenter=length(local);
            return 1.0-smoothstep(radius,max(radius+.035,.04),distanceToCenter);
        }
        """;

    internal static void Apply(float[] emission, int width, int height, int sourceWidth,
        int sourceHeight, EmulationVfdStructure structure, int cellSize, int cellGap)
    {
        if (structure == EmulationVfdStructure.Graphic) return;
        var radius = (0.20f + cellSize / 100f * 0.28f) * (1f - cellGap / 100f * 0.72f);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sourceX = (x + 0.5f) * sourceWidth / width;
            var sourceY = (y + 0.5f) * sourceHeight / height;
            var localX = sourceX - MathF.Floor(sourceX) - 0.5f;
            var localY = sourceY - MathF.Floor(sourceY) - 0.5f;
            var distance = MathF.Sqrt(localX * localX + localY * localY);
            var edge = Math.Clamp((radius + 0.035f - distance) / 0.035f, 0f, 1f);
            emission[y * width + x] *= edge * edge * (3f - 2f * edge);
        }
    }
}
