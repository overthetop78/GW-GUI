using GWGUI.Emulation;

namespace GWGUI.Emulation.Bandai.Common.Contracts;

internal sealed record EmulatorCreationContext(string SessionsDirectory, string CorePath, string HostExecutablePath, Func<IAudioOutput?>? AudioOutputFactory, Func<MachineConfiguration, string>? SaveDirectoryResolver);

internal sealed record EmulatorManagementContext(HttpClient HttpClient, string CoreDirectory);
