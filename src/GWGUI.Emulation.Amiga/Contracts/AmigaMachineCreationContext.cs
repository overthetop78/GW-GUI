using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal sealed record AmigaMachineCreationContext(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<AmigaMachineConfiguration, string>? SaveDirectoryResolver);
