namespace GWGUI.Emulation.Atari.Common.Contracts;

public sealed record CoreActiveInstallation(string ReleaseId, string ReleaseVersion);

public sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    string Id,
    string LibraryName,
    string DllName,
    string ArchiveName,
    Uri ArchiveUri,
    Uri SourceUri,
    string InspectedRevision,
    IReadOnlySet<MachineModel> Models);

public sealed record CoreDiagnosticManifest(
    string ReleaseId,
    string ReleaseVersion,
    string DownloadUrl,
    DateTimeOffset DownloadedUtc,
    long ArchiveSize,
    long LibrarySize,
    string LibrarySha256,
    string Architecture,
    string DeclaredVersion,
    IReadOnlyList<string> Exports);

public sealed record CoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath);

public sealed record CoreInstallProgress(long DownloadedBytes, long? TotalBytes)
{
    public double? Fraction => TotalBytes is > 0 ? DownloadedBytes / (double)TotalBytes.Value : null;
}

public sealed record CoreOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<CoreOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null);

public sealed record CoreOptionCategory(string Key, string Name, string? Description);

public sealed record CoreOptionValue(string Value, string Label);

public sealed record CoreRelease(
    Emulator Emulator,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset PublishedUtc,
    long? ExpectedArchiveSize);

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

internal sealed record HostError(
    string Type,
    string Message,
    ErrorCategory? Category,
    ErrorCode? Code,
    IReadOnlyDictionary<string, string> Context);

internal sealed record MemoryDescriptor(ulong Flags, nint Pointer, nuint Offset, nuint Start, nuint Select,
    nuint Disconnect, nuint Length, string? AddressSpace);
