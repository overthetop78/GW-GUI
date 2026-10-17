namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Contracts;

internal sealed record ExternalCoreInfo(Emulator Emulator, string LibraryName, string LibraryVersion, IReadOnlySet<string> Extensions, bool NeedsFullPath, bool BlocksArchiveExtraction);

internal sealed record MemoryDescriptor(ulong Flags, nint Pointer, nuint Offset, nuint Start, nuint Select, nuint Disconnect, nuint Length, string? AddressSpace);

