using System.Net.Http;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Snk.Common.Contracts;

internal sealed record EmulatorCreationContext( string SessionsDirectory, string CorePath, string HostExecutablePath, Func<IAudioOutput?>? AudioOutputFactory, Func<MachineConfiguration, string>? SaveDirectoryResolver);



internal sealed record EmulatorDefinition(Emulator Emulator, string Id, string LibraryName, string DescriptionResourceKey, IReadOnlySet<MachineModel> Models);
