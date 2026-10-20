using System.Net.Http;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Mattel.Common.Contracts;

internal sealed record EmulatorCreationContext(string SessionsDirectory, string CorePath, string HostExecutablePath, Func<IAudioOutput?>? AudioOutputFactory, Func<MachineConfiguration, string>? SaveDirectoryResolver);
