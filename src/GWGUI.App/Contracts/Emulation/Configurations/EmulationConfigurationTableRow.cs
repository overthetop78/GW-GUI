using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Configurations;

internal sealed record EmulationConfigurationTableRow(
    IEmulationModule Module,
    IEmulationConfiguration Configuration,
    string MachineName,
    string Cpu,
    string TotalRam,
    IReadOnlyList<string> ReaderGlyphs,
    IReadOnlyList<string> PeripheralGlyphs);
