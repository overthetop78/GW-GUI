using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed partial class SoftwareEmulationVideoProcessingPipeline : IEmulationVideoProcessingPipeline
{
    private static float[] Resample(float[] source, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, EmulationVideoSampling sampling)
    {
        if (sourceWidth == outputWidth && sourceHeight == outputHeight) return source;
        var output = new float[checked(outputWidth * outputHeight * 3)];
        var scaleX = outputWidth / (float)sourceWidth;
        var scaleY = outputHeight / (float)sourceHeight;
        Parallel.For(0, outputHeight, y =>
        {
            var sourceY = (y + 0.5f) / scaleY - 0.5f;
            for (var x = 0; x < outputWidth; x++)
            {
                var sourceX = (x + 0.5f) / scaleX - 0.5f;
                if (sampling == EmulationVideoSampling.Xbr)
                {
                    FilterXbr.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Xbrz)
                {
                    FilterXbrz.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hqx)
                {
                    FilterHqx.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hq2x)
                {
                    FilterHq2x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hq3x)
                {
                    FilterHq3x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hq4x)
                {
                    FilterHq4x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.TwoXSai)
                {
                    FilterTwoXSai.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.SuperTwoXSai)
                {
                    FilterSuperTwoXSai.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.SuperEagle)
                {
                    FilterSuperEagle.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.EpxScale2x)
                {
                    FilterEpxScale2x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }                if (sampling == EmulationVideoSampling.ScaleFx)
                {
                    FilterScaleFx.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.ScaleNx)
                {
                    FilterScaleNx.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f, scaleX, scaleY,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Sabr)
                {
                    FilterSabr.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                for (var channel = 0; channel < 3; channel++)
                {
                    output[(y * outputWidth + x) * 3 + channel] = sampling switch
                    {
                        EmulationVideoSampling.Nearest =>
                            FilterNormal.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.Bilinear =>
                            FilterBilinear.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.SharpBilinear =>
                            FilterSharpBilinear.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, scaleX, scaleY, channel),
                        EmulationVideoSampling.Bicubic =>
                            FilterBicubic.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.Jinc2 =>
                            FilterJinc2.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.Lanczos =>
                            FilterLanczos.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),                        _ => throw new ArgumentOutOfRangeException(nameof(sampling), sampling, null)
                    };
                }
            }
        });
        return output;
    }



    private static float Sample(float[] source, int width, int height,
        int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return source[(y * width + x) * 3 + channel];
    }

    private static float Lerp(float first, float second, float amount) =>
        first + (second - first) * amount;

}
