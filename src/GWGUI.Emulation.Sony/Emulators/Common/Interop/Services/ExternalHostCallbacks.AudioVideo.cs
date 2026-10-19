using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

using System.Runtime.InteropServices;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    private void HandleVideo(nint data, uint width, uint height, nuint pitch) =>
        LatestVideoFrame = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.CoreVideoFunctions.Read(
            data, width, height, pitch, HardwareRender, _pixelFormat, _aspectRatio,
            ref _videoSequence, _clock.Elapsed) ?? LatestVideoFrame;

    internal void ApplyInitialAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ApplySystemAvInfo(info,
            ApplyGeometry, value => FramesPerSecond = value, value => SampleRate = value);
    }

    private bool ApplyGeometry(nint data)
    {
        return GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.Geometry>(data, ApplyGeometry);
    }

    private void ApplyGeometry(ExternalCoreApi.Geometry geometry)
    {
        HardwareRender.Resize(checked((int)geometry.MaximumWidth), checked((int)geometry.MaximumHeight));
        GWGUI.Emulation.Functions.EmulationVideoFunctions.ApplyGeometry(geometry, ref _aspectRatio);
    }

    private bool ApplySystemAvInfo(nint data)
    {
        return GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.SystemAvInfo>(data, ApplyInitialAvInfo);
    }

    private void HandleAudioSample(short left, short right)
    {
        PublishAudio(new AudioChunk(GWGUI.Emulation.Functions.EmulationAudioFunctions.SingleFrame(left, right), SampleRate, 1, ++_audioSequence, _clock.Elapsed));
    }

    private nuint HandleAudioBatch(nint data, nuint frames)
    {
        if (data == nint.Zero || frames == ExternalCoreInteropConstants.EmptyNativeSize) return frames;
        var samples = GWGUI.Emulation.Functions.EmulationAudioFunctions.CopyBatch(data, frames);
        PublishAudio(new AudioChunk(samples, SampleRate, checked((int)frames), ++_audioSequence, _clock.Elapsed));
        return frames;
    }

    private void PublishAudio(AudioChunk chunk)
    {
        LatestAudioChunk = chunk;
        _audioBuffer.EnqueueNative(chunk, this, static owner => owner.SampleRate, multichannel: !_legacyCallbacks);
    }

    private nuint HandleMultichannelAudioBatch(nint data, nuint frames, uint channels, uint mask)
    {
        if (data == nint.Zero || frames > MultichannelAudioConstants.MaximumFramesPerBatch
            || !AudioChannelLayoutFunctions.TryFromNative(mask, channels, out var layout)) return 0;
        if (frames == 0) return 0;
        var samples = GWGUI.Emulation.Functions.EmulationAudioFunctions.CopyBatch(data, frames, channels);
        PublishAudio(new AudioChunk(samples, SampleRate, (int)frames, ++_audioSequence, _clock.Elapsed, layout));
        return frames;
    }

    internal bool TryDequeueAudio(out AudioChunk? chunk)
    {
        return _audioBuffer.TryDequeueNative(out chunk);
    }

}
