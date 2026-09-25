using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Contracts;

internal sealed record EmulatorCreationContext(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<MachineConfiguration, string>? SaveDirectoryResolver);

internal sealed record EmulatorManagementContext(
    HttpClient HttpClient,
    string CoreDirectory);

internal sealed record EmulatorPreparedContent(
    MediaConfiguration Configuration,
    string RuntimePath,
    bool NeedsFullPath,
    IReadOnlyDictionary<string, string> RuntimeOptions,
    SessionMedia? SessionMedia = null,
    SessionMedia? BootMedia = null,
    IReadOnlyCollection<string>? ActivityPaths = null,
    bool RequiresDiskControl = false,
    object? State = null);
