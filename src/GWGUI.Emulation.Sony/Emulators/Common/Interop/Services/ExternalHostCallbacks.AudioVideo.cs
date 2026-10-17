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
        HardwareRender.Resize(checked((int)geometry.MaximumWidth), checked((int)geometry.MaximumHeight));
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
        PublishAudio(new AudioChunk(new[] { left, right }, SampleRate, 1, ++_audioSequence, _clock.Elapsed));
    }

    private nuint HandleAudioBatch(nint data, nuint frames)
    {
        if (data == nint.Zero || frames == ExternalCoreInteropConstants.EmptyNativeSize) return frames;
        var samples = new short[checked((int)frames * 2)];
        Marshal.Copy(data, samples, BufferConstants.FirstBufferIndex, samples.Length);
        PublishAudio(new AudioChunk(samples, SampleRate, checked((int)frames), ++_audioSequence, _clock.Elapsed));
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
                var retainedSamples = chunk.InterleavedSamples.Slice((chunk.FrameCount - maximumFrames) * chunk.ChannelCount).ToArray();
                chunk = chunk with { InterleavedSamples = retainedSamples, FrameCount = maximumFrames };
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

    private nuint HandleMultichannelAudioBatch(nint data, nuint frames, uint channels, uint mask)
    {
        if (data == nint.Zero || frames > MultichannelAudioConstants.MaximumFramesPerBatch
            || !AudioChannelLayoutFunctions.TryFromNative(mask, channels, out var layout)) return 0;
        if (frames == 0) return 0;
        var samples = new short[checked((int)frames * (int)channels)];
        Marshal.Copy(data, samples, 0, samples.Length);
        PublishAudio(new AudioChunk(samples, SampleRate, (int)frames, ++_audioSequence, _clock.Elapsed, layout));
        return frames;
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
