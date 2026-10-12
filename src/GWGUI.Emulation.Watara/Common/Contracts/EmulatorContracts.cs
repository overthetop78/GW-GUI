using System.Net.Http;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Watara.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Watara.Common.Contracts;

internal sealed record EmulatorCreationContext(string SessionsDirectory, string CorePath, string HostExecutablePath, Func<IAudioOutput?>? AudioOutputFactory, Func<MachineConfiguration, string>? SaveDirectoryResolver);
internal sealed record EmulatorManagementContext(HttpClient HttpClient, string CoreDirectory);
