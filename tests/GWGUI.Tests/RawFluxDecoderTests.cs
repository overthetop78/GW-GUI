using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les bornes et anomalies temporelles de l'analyse du flux brut.</summary>
public sealed class RawFluxDecoderTests
{
    [Fact]
    public void EmptyFluxProducesOnlyTheFallbackCellEstimate()
    {
        var result = new RawFluxDecoder().Decode(new FluxRevolution(0, []));

        Assert.True(result.EstimatedBitCellTicks > 0);
        Assert.Empty(result.Sectors);
        Assert.Empty(result.DecodedBytes);
        Assert.Empty(result.Structures);
    }

    [Fact]
    public void CellConversionUsesOneAndSixtyFourAsBounds()
    {
        Assert.Equal(RawFluxAnalysisDefinitions.MinimumCellCount, RawFluxDecoder.ConvertToCellCount(0, 40));
        Assert.Equal(RawFluxAnalysisDefinitions.MaximumCellCount, RawFluxDecoder.ConvertToCellCount(10_000, 40));
    }

    [Fact]
    public void LongIntervalThresholdIsStrict()
    {
        Assert.Equal(RawFluxAnomalyKind.None, RawFluxDecoder.Classify(400, 40, 1));
        Assert.Equal(RawFluxAnomalyKind.LongInterval, RawFluxDecoder.Classify(401, 40, 1));
    }

    [Fact]
    public void ShortPulseThresholdIsStrictAndExcludesFirstInterval()
    {
        Assert.Equal(RawFluxAnomalyKind.None, RawFluxDecoder.Classify(22, 40, 1));
        Assert.Equal(RawFluxAnomalyKind.ShortPulse, RawFluxDecoder.Classify(21, 40, 1));
        Assert.Equal(RawFluxAnomalyKind.None, RawFluxDecoder.Classify(1, 40, 0));
    }

    [Fact]
    public void ResultPreservesOffsetsLengthsDescriptionsConfidenceAndTickUnits()
    {
        var intervals = new List<uint> { 40 };
        intervals.AddRange(Enumerable.Repeat((uint)40, 10));
        intervals.Add(201);
        intervals.Add(10);
        var result = new RawFluxDecoder().Decode(new FluxRevolution(8_000_000, intervals));

        Assert.Equal(20, result.EstimatedBitCellTicks);
        Assert.Equal(RawFluxAnalysisDefinitions.Confidence, result.Confidence);
        Assert.Collection(result.Structures,
            anomaly => { Assert.Equal(22, anomaly.BitOffset); Assert.Equal(10, anomaly.BitLength); Assert.Contains(RawFluxAnalysisDefinitions.LongIntervalDescription, anomaly.Description); },
            anomaly => { Assert.Equal(32, anomaly.BitOffset); Assert.Equal(1, anomaly.BitLength); Assert.Contains(RawFluxAnalysisDefinitions.ShortPulseDescription, anomaly.Description); });
    }
}
