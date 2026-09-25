using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable
{
private void OnVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (data == nint.Zero)
        {
            if (LatestVideoFrame is { } previous)
                LatestVideoFrame = previous with
                {
                    Sequence = ++_videoSequence,
                    Timestamp = VideoFunctions.Timestamp(_videoStartTimestamp)
                };
            return;
        }
        if (width == CommonConstants.EmptyFrameDimension ||
            height == CommonConstants.EmptyFrameDimension || pitch == CommonConstants.EmptyNativeSize) return;
        var length = VideoFunctions.FrameLength(height, pitch);
        if (length > EmulationHostProtocolConstants.VideoSlotCapacity) return;
        var pixels = _videoBuffers.Rent(length);
        VideoFunctions.CopyRows(data, pixels, checked((int)height), checked((int)pitch));
        if (_emulator == Emulator.Hatari && !_usesNativeLedInterface)
            EmulationMediaActivityFunctions.CaptureHatariOverlay(pixels.AsSpan(CommonConstants.FirstBufferIndex, length),
                checked((int)width), checked((int)height), checked((int)pitch), _pixelFormat, _ledStates);
        else if (_emulator == Emulator.Atari800)
            EmulationMediaActivityFunctions.CaptureAtari800Overlay(
                pixels.AsSpan(CommonConstants.FirstBufferIndex, length), checked((int)width), checked((int)height),
                checked((int)pitch), _pixelFormat, _ledStates);
        LatestVideoFrame = new VideoFrame(pixels.AsMemory(CommonConstants.FirstBufferIndex, length),
            checked((int)width), checked((int)height), checked((int)pitch), _pixelFormat, AspectRatio,
            ++_videoSequence, VideoFunctions.Timestamp(_videoStartTimestamp));
    }

    private void OnAudioSample(short left, short right)
    {
        AddAudio(AudioFunctions.SingleFrame(left, right), AudioConstants.SingleFrameCount);
    }

    private nuint OnAudioBatch(nint data, nuint frames)
    {
        if (data == nint.Zero || frames == CommonConstants.EmptyNativeSize ||
            frames > AudioConstants.MaximumFramesPerBatch) return CommonConstants.EmptyNativeSize;
        var frameCount = checked((int)frames);
        var samples = AudioFunctions.CopyBatch(data, frameCount);
        AddAudio(samples, frameCount);
        return frames;
    }

    private void AddAudio(ReadOnlyMemory<short> samples, int frameCount)
    {
        var chunk = new AudioChunk(samples, SampleRate, frameCount, ++_audioSequence, TimeSpan.Zero);
        LatestAudioChunk = chunk;
        _audio.Enqueue(chunk);
    }

    private void OnInputPoll()
    {
        _input.Poll();
        _keyboard.Publish(_input.Polled.Keys, _keyboardEvent);
    }
    private short OnInputState(uint port, uint device, uint index, uint id) => _input.State(port, device, index, id);
    private void OnSetLedState(int led, int state) => _ledStates[led] = state != CommonConstants.InactiveState;
    private void OnFileRead(string path, long length)
    {
        if (length > 0 && _emulator == Emulator.VirtualJaguar && _opticalActivityPaths.Contains(path))
            _ledStates[0] = true;
    }
    private bool OnSetRumbleState(uint port, uint effect, ushort strength) => false;
    private bool OnSetSensorState(uint port, uint action, uint rate) => false;
    private float OnGetSensorInput(uint port, uint id) => EnvironmentConstants.NoSensorInput;

    private void OnLog(int level, nint format)
    {
        var text = EnvironmentFunctions.CopyNativeLogTemplate(format);
        if (!string.IsNullOrEmpty(text)) Diagnostics.Add(text);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _optionHost.Dispose();
        _videoBuffers.Dispose();
        _virtualFileSystem.Dispose();
        _systemDirectory.Dispose();
        _contentDirectory.Dispose();
        _saveDirectory.Dispose();
        _assetsDirectory.Dispose();
    }
}
