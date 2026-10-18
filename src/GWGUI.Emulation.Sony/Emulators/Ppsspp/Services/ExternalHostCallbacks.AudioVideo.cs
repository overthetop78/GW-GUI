using ExternalCoreInteropConstants = GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ExternalCoreInteropConstants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

using System.Runtime.InteropServices;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

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
                var retainedSamples = chunk.InterleavedSamples.Slice((chunk.FrameCount - maximumFrames) * 2).ToArray();
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
