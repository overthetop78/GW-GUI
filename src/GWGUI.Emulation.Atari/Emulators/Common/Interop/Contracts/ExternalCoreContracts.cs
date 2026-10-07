namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Contracts;

internal sealed record ExternalCoreExports(
    ExternalCoreApi.SetEnvironment SetEnvironment,
    ExternalCoreApi.SetVideo SetVideo,
    ExternalCoreApi.SetAudioSample SetAudioSample,
    ExternalCoreApi.SetAudioBatch SetAudioBatch,
    ExternalCoreApi.SetInputPoll SetInputPoll,
    ExternalCoreApi.SetInputState SetInputState,
    ExternalCoreApi.VoidCall Initialize,
    ExternalCoreApi.VoidCall Deinitialize,
    ExternalCoreApi.GetSystemInfo GetSystemInfo,
    ExternalCoreApi.GetSystemAvInfo GetSystemAvInfo,
    ExternalCoreApi.SetControllerPortDevice SetControllerPortDevice,
    ExternalCoreApi.VoidCall Reset,
    ExternalCoreApi.VoidCall Run,
    ExternalCoreApi.LoadGame LoadGame,
    ExternalCoreApi.VoidCall UnloadGame,
    ExternalCoreApi.GetRegion GetRegion,
    ExternalCoreApi.GetMemoryData GetMemoryData,
    ExternalCoreApi.GetMemorySize GetMemorySize,
    ExternalCoreApi.GetSerializedSize GetSerializedSize,
    ExternalCoreApi.Serialize Serialize,
    ExternalCoreApi.Serialize Unserialize);

internal sealed record ExternalCoreInfo(
    Emulator Emulator,
    string LibraryName,
    string LibraryVersion,
    IReadOnlySet<string> Extensions,
    bool NeedsFullPath,
    bool BlocksArchiveExtraction);

internal sealed record MemoryDescriptor(ulong Flags, nint Pointer, nuint Offset, nuint Start, nuint Select,
    nuint Disconnect, nuint Length, string? AddressSpace);
