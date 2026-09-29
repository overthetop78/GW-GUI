using GWGUI.Emulation.Sega.Emulators.Flycast.Constants;
using GWGUI.Emulation.Sega.Emulators.Flycast.Contracts;
using GWGUI.Emulation.Sega.Emulators.Flycast.Factories;
using GWGUI.Emulation.Sega.Emulators.Flycast.Functions;
using GWGUI.Emulation.Sega.Emulators.Flycast.Services;

using System.Runtime.InteropServices;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Services;

internal sealed partial class ExternalHostCallbacks
{
    private void HandleVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (width == ExternalCoreInteropConstants.EmptyFrameDimension
            || height == ExternalCoreInteropConstants.EmptyFrameDimension) return;
        if (data == HardwareFrameBufferValid)
        {
            if (_hardwareRenderContext?.TryReadFramebuffer(checked((int)width), checked((int)height),
                    out var hardwarePixels) != true) return;
            LatestVideoFrame = new VideoFrame(hardwarePixels, checked((int)width), checked((int)height),
                checked((int)width * HardwarePixelSize), EmulationPixelFormat.Xrgb8888,
                _aspectRatio > 0 ? _aspectRatio : width / (float)height,
                ++_videoSequence, _clock.Elapsed);
            return;
        }
        if (data == nint.Zero) return;
        var byteCount = checked((int)(pitch * height));
        var pixels = new byte[byteCount];
        Marshal.Copy(data, pixels, BufferConstants.FirstBufferIndex, byteCount);
        LatestVideoFrame = new VideoFrame(pixels, checked((int)width), checked((int)height),
            checked((int)pitch), _pixelFormat, _aspectRatio > 0 ? _aspectRatio : width / (float)height,
            ++_videoSequence, _clock.Elapsed);
    }

    private static readonly nint HardwareFrameBufferValid = new(-1);
    private const int HardwarePixelSize = 4;

    internal void ApplyInitialAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        ApplyGeometry(info.Geometry);
        if (double.IsFinite(info.Timing.FramesPerSecond) && info.Timing.FramesPerSecond > 0)
            FramesPerSecond = info.Timing.FramesPerSecond;
        if (double.IsFinite(info.Timing.SampleRate) && info.Timing.SampleRate is > 0 and <= int.MaxValue)
            SampleRate = checked((int)Math.Round(info.Timing.SampleRate));
    }

    private bool ApplyGeometry(nint data)
    {
        if (data == 0) return false;
        ApplyGeometry(Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data));
        return true;
    }

    private void ApplyGeometry(ExternalCoreApi.Geometry geometry)
    {
        if (float.IsFinite(geometry.AspectRatio) && geometry.AspectRatio > 0)
            _aspectRatio = geometry.AspectRatio;
        else if (geometry.BaseHeight > 0)
            _aspectRatio = geometry.BaseWidth / (float)geometry.BaseHeight;
    }

    private bool ApplySystemAvInfo(nint data)
    {
        if (data == 0) return false;
        ApplyInitialAvInfo(Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(data));
        return true;
    }

    private void HandleAudioSample(short left, short right)
    {
        PublishAudio(new AudioChunk(new[] { left, right }, SampleRate, 1,
            ++_audioSequence, _clock.Elapsed));
    }

    private nuint HandleAudioBatch(nint data, nuint frames)
    {
        if (data == nint.Zero || frames == ExternalCoreInteropConstants.EmptyNativeSize) return frames;
        var samples = new short[checked((int)frames * 2)];
        Marshal.Copy(data, samples, BufferConstants.FirstBufferIndex, samples.Length);
        PublishAudio(new AudioChunk(samples, SampleRate, checked((int)frames),
            ++_audioSequence, _clock.Elapsed));
        return frames;
    }

    private void PublishAudio(AudioChunk chunk)
    {
        LatestAudioChunk = chunk;
        lock (_audioGate)
        {
            var maximumFrames = Math.Max(1, SampleRate / 5);
            if (chunk.FrameCount > maximumFrames)
            {
                var retainedSamples = chunk.InterleavedStereo.Slice((chunk.FrameCount - maximumFrames) * 2).ToArray();
                chunk = new AudioChunk(retainedSamples, chunk.SampleRate, maximumFrames, chunk.Sequence, chunk.Timestamp);
                AudioOverrunCount++;
            }
            _audioChunks.Enqueue(chunk);
            _bufferedAudioFrames += chunk.FrameCount;
            while (_bufferedAudioFrames > maximumFrames && _audioChunks.Count > 1)
            {
                _bufferedAudioFrames -= _audioChunks.Dequeue().FrameCount;
                AudioOverrunCount++;
            }
        }
    }

    internal bool TryDequeueAudio(out AudioChunk? chunk)
    {
        lock (_audioGate)
        {
            if (!_audioChunks.TryDequeue(out chunk)) return false;
            _bufferedAudioFrames -= chunk.FrameCount;
            return true;
        }
    }

}

