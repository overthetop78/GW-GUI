using GWGUI.Emulation;

namespace GWGUI.Emulation.Nec.Common.Contracts;

internal sealed record EmulatorCreationContext(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<MachineConfiguration, string>? SaveDirectoryResolver,
    string FirmwareDirectory);

internal sealed record EmulatorManagementContext(
    HttpClient HttpClient,
    string CoreDirectory);

