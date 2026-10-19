namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

internal sealed record CoreDefinition(
    string Id,
    string LibraryName,
    string LibraryFileName,
    Func<IReadOnlyList<MediaConfiguration>, string, string, string?>? PrepareContent = null,
    IReadOnlyDictionary<int, uint>? Subsystems = null,
    string? FirmwareFileName = null,
    string ErrorResourcePrefix = "Emulation.Error.Core",
    Func<string>? FullContentPathsRequired = null,
    Action<ExternalCoreApi.GetMemoryData, ExternalCoreApi.GetMemorySize, string, MachineConfiguration>? LoadSaveRam = null,
    Action<ExternalCoreApi.GetMemoryData, ExternalCoreApi.GetMemorySize, string, MachineConfiguration>? SaveSaveRam = null,
    string? TransportDisplayName = null);
