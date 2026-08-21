namespace GWGUI.Emulation.Atari;

internal sealed record AtariMemoryDescriptor(ulong Flags, nint Pointer, nuint Offset, nuint Start, nuint Select,
    nuint Disconnect, nuint Length, string? AddressSpace);
