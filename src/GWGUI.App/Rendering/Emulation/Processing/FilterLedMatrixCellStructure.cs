using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedMatrixCellStructure
{
    internal const string Shader = """
        float filterLedMatrixPitch(float size){return 1.0+floor(clamp(size,0.0,1.0)*7.0+.5);}
        float filterLedMatrixDistance(vec2 localPosition,float shape)
        {
            return shape<.5?length(localPosition):max(abs(localPosition.x),abs(localPosition.y));
        }
        float filterLedMatrixCore(float distance,float gap,float edgeWidth)
        {
            float radius=.5*(1.0-clamp(gap,0.0,1.0)*.86);
            return smoothstep(radius+edgeWidth,radius-edgeWidth,distance);
        }
        """;

    internal static LedMatrixCellMap Create(float[] source, int sourceWidth,
        int sourceHeight, int outputWidth, int outputHeight, int cellSize, int cellGap,
        EmulationLedMatrixShape shape, int haloRadius)
    {
        var pitch = 1 + (int)MathF.Round(Math.Clamp(cellSize, 0, 100) / 100f * 7f);
        var columns = (sourceWidth + pitch - 1) / pitch;
        var rows = (sourceHeight + pitch - 1) / pitch;
        var averages = new float[checked(columns * rows * 3)];
        var counts = new int[checked(columns * rows)];
        for (var y = 0; y < outputHeight; y++)
        for (var x = 0; x < outputWidth; x++)
        {
            var sourceX = Math.Min(sourceWidth - 1, x * sourceWidth / outputWidth);
            var sourceY = Math.Min(sourceHeight - 1, y * sourceHeight / outputHeight);
            var cell = (sourceY / pitch * columns) + sourceX / pitch;
            var pixel = (y * outputWidth + x) * 3;
            averages[cell * 3] += source[pixel];
            averages[cell * 3 + 1] += source[pixel + 1];
            averages[cell * 3 + 2] += source[pixel + 2];
            counts[cell]++;
        }
        for (var cell = 0; cell < counts.Length; cell++)
        {
            var count = Math.Max(1, counts[cell]);
            averages[cell * 3] /= count;
            averages[cell * 3 + 1] /= count;
            averages[cell * 3 + 2] /= count;
        }

        var emission = new float[source.Length];
        var core = new float[checked(outputWidth * outputHeight)];
        var halo = new float[core.Length];
        var radius = .5f * (1f - Math.Clamp(cellGap, 0, 100) / 100f * .86f);
        var haloExtent = .04f + Math.Clamp(haloRadius, 0, 100) / 100f * .9f;
        var edge = MathF.Max(sourceWidth / (float)outputWidth,
            sourceHeight / (float)outputHeight) / pitch;
        for (var y = 0; y < outputHeight; y++)
        for (var x = 0; x < outputWidth; x++)
        {
            var sourceX = (x + .5f) * sourceWidth / outputWidth;
            var sourceY = (y + .5f) * sourceHeight / outputHeight;
            var cellX = Math.Min(columns - 1, (int)(sourceX / pitch));
            var cellY = Math.Min(rows - 1, (int)(sourceY / pitch));
            var localX = sourceX / pitch - MathF.Floor(sourceX / pitch) - .5f;
            var localY = sourceY / pitch - MathF.Floor(sourceY / pitch) - .5f;
            var distance = shape == EmulationLedMatrixShape.Round
                ? MathF.Sqrt(localX * localX + localY * localY)
                : MathF.Max(MathF.Abs(localX), MathF.Abs(localY));
            var pixelIndex = y * outputWidth + x;
            core[pixelIndex] = SmoothStep(radius + edge, radius - edge, distance);
            var outside = MathF.Max(0f, distance - radius);
            halo[pixelIndex] = MathF.Exp(-outside * 4f / haloExtent)
                * (1f - core[pixelIndex]);
            var average = (cellY * columns + cellX) * 3;
            var pixel = pixelIndex * 3;
            emission[pixel] = averages[average];
            emission[pixel + 1] = averages[average + 1];
            emission[pixel + 2] = averages[average + 2];
        }
        return new(emission, core, halo);
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var position = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return position * position * (3f - 2f * position);
    }
}
