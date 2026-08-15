namespace GWGUI.Emulation;

public enum EmulationMachineState { Created, Starting, Running, Paused, Stopping, Stopped, Faulted }

public interface IEmulatedMachine : IAsyncDisposable
{
    Guid Id { get; }
    EmulationMachineState State { get; }
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask PauseAsync(CancellationToken cancellationToken = default);
    ValueTask ResumeAsync(CancellationToken cancellationToken = default);
    ValueTask SoftResetAsync(CancellationToken cancellationToken = default);
    ValueTask HardResetAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}

public interface IEmulationEngine<in TConfiguration>
{
    IEmulatedMachine CreateMachine(TConfiguration configuration);
}

public enum EmulationPixelFormat { Rgb565, Xrgb8888 }

public enum EmulationVideoRenderer
{
    Direct3D11,
    Vulkan,
    OpenGL,
    Wpf
}

public sealed record VideoFrame(ReadOnlyMemory<byte> Pixels, int Width, int Height, int Pitch,
    EmulationPixelFormat PixelFormat, float AspectRatio, long Sequence, TimeSpan Timestamp);

public sealed record AudioChunk(ReadOnlyMemory<short> InterleavedStereo, int SampleRate,
    int FrameCount, long Sequence, TimeSpan Timestamp)
{
    public bool HasValidLength => InterleavedStereo.Length == FrameCount * 2;
}

public interface IAudioOutput : IDisposable
{
    void Start(int sampleRate);
    void Write(ReadOnlySpan<short> interleavedStereo);
    void Flush();
    void Stop();
}

public enum EmulationMediaSlot { Floppy0, Floppy1, Floppy2, Floppy3, HardDisk0, Cd0 }
public enum EmulationMediaType { Floppy, HardDisk, CompactDisc, Directory }
public sealed record EmulationMedia(string Path, EmulationMediaSlot Slot, EmulationMediaType Type,
    bool IsReadOnly, bool IsInserted);

public enum EmulationKey
{
    Unknown, Backspace, Tab, Return, Escape, Space,
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    Left, Right, Up, Down, LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, Delete, Insert, Home, End, PageUp, PageDown,
    Comma, Period, Slash, Backslash, Minus, Equals, Semicolon, Quote, LeftBracket, RightBracket,
    Backquote, CapsLock, Help, LeftAmiga, RightAmiga,
    Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9,
    NumpadPeriod, NumpadDivide, NumpadMultiply, NumpadMinus, NumpadPlus, NumpadEnter
}

public sealed record EmulationPointerState(int DeltaX, int DeltaY, int Wheel, bool Left, bool Right, bool Middle);
public sealed record EmulationControllerState(uint Buttons, short LeftX, short LeftY, short RightX,
    short RightY, short LeftTrigger, short RightTrigger)
{
    public static EmulationControllerState Empty { get; } = new(0, 0, 0, 0, 0, 0, 0);
}

public sealed record EmulationInputSnapshot(IReadOnlySet<EmulationKey> Keys,
    EmulationPointerState Pointer, IReadOnlyList<EmulationControllerState> Controllers)
{
    public static EmulationInputSnapshot Empty { get; } = new(new HashSet<EmulationKey>(),
        new EmulationPointerState(0, 0, 0, false, false, false),
        [EmulationControllerState.Empty, EmulationControllerState.Empty,
         EmulationControllerState.Empty, EmulationControllerState.Empty]);
}
