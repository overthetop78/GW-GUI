using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Contracts;

internal sealed record EmulatorCreationContext(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<AtariMachineConfiguration, string>? SaveDirectoryResolver);
