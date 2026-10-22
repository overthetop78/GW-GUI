using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Commodore.Common.Functions.AudioFunctions;

using System.Runtime.InteropServices;
using GWGUI.Emulation;

using static GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    private void HandleVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (data == nint.Zero || width == ExternalCoreInteropConstants.EmptyFrameDimension
            || height == ExternalCoreInteropConstants.EmptyFrameDimension) return;
        var pixels = GWGUI.Emulation.Functions.EmulationVideoFunctions.CopyFramePixels(data, height, pitch);
        LatestVideoFrame = new VideoFrame(pixels, checked((int)width), checked((int)height), checked((int)pitch), _pixelFormat, _aspectRatio > MinimumAspectRatio ? _aspectRatio : width / (float)height, ++_videoSequence, _clock.Elapsed);
    }

    internal void ApplyInitialAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        CommonCoreHostFunctions.EnvironmentProcessing.ApplySystemAvInfo(info,
            ApplyGeometry, value => FramesPerSecond = value, value => SampleRate = value);
    }

    private bool ApplyGeometry(nint data)
    {
        return CommonCoreHostFunctions.EnvironmentProcessing.ApplyStructure<ExternalCoreApi.Geometry>(data, ApplyGeometry);
    }

    private void ApplyGeometry(ExternalCoreApi.Geometry geometry)
    {
        GWGUI.Emulation.Functions.EmulationVideoFunctions.ApplyGeometry(geometry, ref _aspectRatio);
    }

    private bool ApplySystemAvInfo(nint data)
    {
        return CommonCoreHostFunctions.EnvironmentProcessing.ApplyStructure<ExternalCoreApi.SystemAvInfo>(data, ApplyInitialAvInfo);
    }

    private void HandleAudioSample(short left, short right)
    {
        PublishAudio(new AudioChunk(AudioFunctions.Processing.SingleFrame(left, right), SampleRate, AudioConstants.SingleFrameCount, ++_audioSequence, _clock.Elapsed));
    }

    private nuint HandleAudioBatch(nint data, nuint frames)
    {
        if (data == nint.Zero || frames == ExternalCoreInteropConstants.EmptyNativeSize) return frames;
        var samples = AudioFunctions.Processing.CopyBatch(data, frames);
        PublishAudio(new AudioChunk(samples, SampleRate, checked((int)frames), ++_audioSequence, _clock.Elapsed));
        return frames;
    }

    private void PublishAudio(AudioChunk chunk)
    {
        LatestAudioChunk = chunk;
        _audioBuffer.EnqueueNative(chunk, this, static owner => owner.SampleRate);
    }

    internal bool TryDequeueAudio(out AudioChunk? chunk)
    {
        return _audioBuffer.TryDequeueNative(out chunk);
    }

}
