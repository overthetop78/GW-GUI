using AudioFunctions = GWGUI.Emulation.Sega.Common.Functions.AudioFunctions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

using System.Runtime.InteropServices;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    private void HandleVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (data == nint.Zero || width == ExternalCoreInteropConstants.EmptyFrameDimension
            || height == ExternalCoreInteropConstants.EmptyFrameDimension) return;
        if (data == Constants.HardwareRenderConstants.HardwareFramebuffer)
        {
            if (_hardwareRenderContext?.TryReadFramebuffer(checked((int)width), checked((int)height), out var frame) != true) return;
            LatestVideoFrame = new VideoFrame(frame, checked((int)width), checked((int)height),
                checked((int)width * Constants.HardwareRenderConstants.PixelByteCount),
                EmulationPixelFormat.Xrgb8888, _aspectRatio > 0 ? _aspectRatio : width / (float)height,
                ++_videoSequence, _clock.Elapsed);
            return;
        }
        var pixels = GWGUI.Emulation.Functions.EmulationVideoFunctions.CopyFramePixels(data, height, pitch);
        LatestVideoFrame = new VideoFrame(pixels, checked((int)width), checked((int)height), checked((int)pitch), _pixelFormat, _aspectRatio > 0 ? _aspectRatio : width / (float)height, ++_videoSequence, _clock.Elapsed);
    }

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
        _hardwareRenderContext?.EnsureFramebuffer(checked((int)geometry.MaximumWidth), checked((int)geometry.MaximumHeight));
        GWGUI.Emulation.Functions.EmulationVideoFunctions.ApplyGeometry(geometry, ref _aspectRatio);
    }

    private bool ApplySystemAvInfo(nint data)
    {
        return GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ApplyStructure<ExternalCoreApi.SystemAvInfo>(data, ApplyInitialAvInfo);
    }

    private void HandleAudioSample(short left, short right)
    {
        PublishAudio(new AudioChunk(AudioFunctions.Processing.SingleFrame(left, right), SampleRate, 1, ++_audioSequence, _clock.Elapsed));
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



