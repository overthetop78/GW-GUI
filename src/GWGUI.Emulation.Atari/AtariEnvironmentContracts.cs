namespace GWGUI.Emulation.Atari;

internal sealed record AtariInputDescriptor(uint Port, uint Device, uint Index, uint Id, string Description);
internal sealed record AtariControllerDevice(string Description, uint Id);
internal sealed record AtariControllerPort(IReadOnlyList<AtariControllerDevice> Devices);
internal sealed record AtariMemoryDescriptor(ulong Flags, nint Pointer, nuint Offset, nuint Start, nuint Select,
    nuint Disconnect, nuint Length, string? AddressSpace);
internal sealed record AtariEnvironmentMessage(string Text, uint Frames);
internal sealed record AtariEnvironmentExtendedMessage(string Text, uint DurationMilliseconds, uint Priority,
    uint Level, uint Target, uint Type, sbyte Progress);
