using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed partial class SoftwareEmulationVideoProcessingPipeline : IEmulationVideoProcessingPipeline
{
    public void ResetTemporalHistory()
    {
        _interlacing.Reset();
        _signalTimestamp = TimeSpan.Zero;
        _signalSequence = 0;
        ResetHistory();
        _plasmaPersistence.Reset();
        ResetVectorHistory();
        _vfdPersistence.Reset();
        ResetDotMatrixHistory();
        ResetSegmentDisplayHistory();
        ResetEPaperHistory();
        _motionBlur.Reset();
        _generalPersistence.Reset();
    }

    private void ApplyFixedPixelTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.FixedPixel)
        {
            ResetHistory();
            return;
        }

        var compatibleHistory = _fixedPixelHistory is not null
            && _historyWidth == width && _historyHeight == height
            && timestamp >= _historyTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _historyTimestamp).TotalMilliseconds);
            var response = FilterFixedPixelResponse.BlendFactor(
                configuration.FixedPixel.ResponseTimeMilliseconds, elapsedMilliseconds);
            for (var index = 0; index < colors.Length; index++)
            {
                var responded = FilterFixedPixelResponse.Apply(
                    _fixedPixelHistory![index], colors[index], response);
                colors[index] = FilterFixedPixelPersistence.Apply(
                    responded, _fixedPixelHistory[index],
                    configuration.FixedPixel.PersistenceIntensity);
            }
        }

        _fixedPixelHistory = colors.ToArray();
        _historyWidth = width;
        _historyHeight = height;
        _historyTimestamp = timestamp;
    }

    private void ResetHistory()
    {
        _fixedPixelHistory = null;
        _historyWidth = 0;
        _historyHeight = 0;
        _historyTimestamp = TimeSpan.Zero;
    }

    private void ApplyVectorTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, long sequence)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.Vector)
        {
            ResetVectorHistory();
            return;
        }
        var compatibleHistory = _vectorHistory is not null
            && _vectorHistoryWidth == width && _vectorHistoryHeight == height
            && sequence >= _vectorHistorySequence;
        if (compatibleHistory)
            FilterVectorPersistence.Apply(colors, _vectorHistory!,
                configuration.Vector.PersistenceIntensity);
        _vectorHistory = colors.ToArray();
        _vectorHistoryWidth = width;
        _vectorHistoryHeight = height;
        _vectorHistorySequence = sequence;
    }

    private void ResetVectorHistory()
    {
        _vectorHistory = null;
        _vectorHistoryWidth = 0;
        _vectorHistoryHeight = 0;
        _vectorHistorySequence = 0;
    }

    private void ApplyDotMatrixTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.DotMatrix)
        {
            ResetDotMatrixHistory();
            return;
        }
        var compatibleHistory = _dotMatrixHistory is not null
            && _dotMatrixHistoryWidth == width && _dotMatrixHistoryHeight == height
            && timestamp >= _dotMatrixHistoryTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _dotMatrixHistoryTimestamp).TotalMilliseconds);
            var response = FilterDotMatrixResponse.BlendFactor(
                configuration.DotMatrix.ResponseTimeMilliseconds, elapsedMilliseconds);
            var reflective = configuration.DotMatrix.Palette is EmulationDotMatrixPalette.Green
                or EmulationDotMatrixPalette.Gray;
            var background = configuration.DotMatrix.Palette == EmulationDotMatrixPalette.Green
                ? (.16f, .25f, .075f) : (.64f, .68f, .62f);
            for (var index = 0; index < colors.Length; index++)
            {
                var responded = Lerp(_dotMatrixHistory![index], colors[index], response);
                colors[index] = FilterDotMatrixPersistence.Apply(responded,
                    _dotMatrixHistory[index], configuration.DotMatrix.PersistenceMilliseconds,
                    elapsedMilliseconds, reflective, index % 3 switch
                    {
                        0 => background.Item1,
                        1 => background.Item2,
                        _ => background.Item3
                    });
            }
        }
        _dotMatrixHistory = colors.ToArray();
        _dotMatrixHistoryWidth = width;
        _dotMatrixHistoryHeight = height;
        _dotMatrixHistoryTimestamp = timestamp;
    }

    private void ResetDotMatrixHistory()
    {
        _dotMatrixHistory = null;
        _dotMatrixHistoryWidth = 0;
        _dotMatrixHistoryHeight = 0;
        _dotMatrixHistoryTimestamp = TimeSpan.Zero;
    }

    private void ApplySegmentDisplayTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.SegmentDisplay)
        {
            ResetSegmentDisplayHistory();
            return;
        }
        var compatibleHistory = _segmentDisplayHistory is not null
            && _segmentDisplayHistoryWidth == width && _segmentDisplayHistoryHeight == height
            && timestamp >= _segmentDisplayHistoryTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _segmentDisplayHistoryTimestamp).TotalMilliseconds);
            var response = FilterSegmentDisplayResponse.BlendFactor(
                configuration.SegmentDisplay.ResponseTimeMilliseconds, elapsedMilliseconds);
            var persistence = FilterSegmentDisplayPersistence.Decay(
                configuration.SegmentDisplay.PersistenceMilliseconds, elapsedMilliseconds);
            for (var index = 0; index < colors.Length; index++)
            {
                var previous = _segmentDisplayHistory![index];
                var target = colors[index];
                colors[index] = target >= previous
                    ? Lerp(previous, target, response)
                    : MathF.Max(target, previous * persistence);
            }
        }
        _segmentDisplayHistory = colors.ToArray();
        _segmentDisplayHistoryWidth = width;
        _segmentDisplayHistoryHeight = height;
        _segmentDisplayHistoryTimestamp = timestamp;
    }

    private void ResetSegmentDisplayHistory()
    {
        _segmentDisplayHistory = null;
        _segmentDisplayHistoryWidth = 0;
        _segmentDisplayHistoryHeight = 0;
        _segmentDisplayHistoryTimestamp = TimeSpan.Zero;
    }

    private void ApplyEPaperTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.EPaper)
        {
            ResetEPaperHistory();
            return;
        }
        var compatibleHistory = _ePaperHistory is not null
            && _ePaperHistoryWidth == width && _ePaperHistoryHeight == height
            && timestamp >= _ePaperHistoryTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = (timestamp - _ePaperHistoryTimestamp).TotalMilliseconds;
            var response = FilterEPaperRefreshTime.BlendFactor(elapsedMilliseconds,
                configuration.EPaper.RefreshTimeMilliseconds);
            var ghosting = FilterEPaperGhosting.BlendFactor(configuration.EPaper.Ghosting);
            for (var index = 0; index < colors.Length; index++)
            {
                var refreshed = Lerp(_ePaperHistory![index], colors[index], response);
                colors[index] = Lerp(_ePaperHistory[index], refreshed, ghosting);
            }
        }
        _ePaperHistory = colors.ToArray();
        _ePaperHistoryWidth = width;
        _ePaperHistoryHeight = height;
        _ePaperHistoryTimestamp = timestamp;
    }

    private void ResetEPaperHistory()
    {
        _ePaperHistory = null;
        _ePaperHistoryWidth = 0;
        _ePaperHistoryHeight = 0;
        _ePaperHistoryTimestamp = TimeSpan.Zero;
    }

}
