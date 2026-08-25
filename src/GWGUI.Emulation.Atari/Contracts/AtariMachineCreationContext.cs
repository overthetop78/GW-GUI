using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariMachineCreationContext(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<AtariMachineConfiguration, string>? SaveDirectoryResolver);
