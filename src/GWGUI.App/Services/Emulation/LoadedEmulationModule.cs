using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Services.Emulation;

internal sealed record LoadedEmulationModule(
    IEmulationModule Module,
    EmulationModuleManifest Manifest,
    string InstallationDirectory);
