namespace GWGUI.Emulation.Contracts;

internal sealed record EmulatorCreationContext<TConfiguration>(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<TConfiguration, string>? SaveDirectoryResolver);
