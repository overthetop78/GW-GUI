using System.Net.Http;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Wenquxing.Common.Contracts;

internal sealed record EmulatorCreationContext(string SessionsDirectory, string CorePath, string HostExecutablePath, Func<IAudioOutput?>? AudioOutputFactory, Func<MachineConfiguration, string>? SaveDirectoryResolver);
