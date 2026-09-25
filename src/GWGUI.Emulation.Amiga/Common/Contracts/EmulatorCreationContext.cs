using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Contracts;

internal sealed record EmulatorCreationContext(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<AmigaMachineConfiguration, string>? SaveDirectoryResolver);
