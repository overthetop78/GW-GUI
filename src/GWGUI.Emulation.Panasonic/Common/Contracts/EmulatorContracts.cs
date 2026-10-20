using System.Net.Http;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Panasonic.Common.Contracts;

internal sealed record EmulatorCreationContext(string SessionsDirectory, string CorePath, string HostExecutablePath, Func<IAudioOutput?>? AudioOutputFactory, Func<MachineConfiguration, string>? SaveDirectoryResolver);
