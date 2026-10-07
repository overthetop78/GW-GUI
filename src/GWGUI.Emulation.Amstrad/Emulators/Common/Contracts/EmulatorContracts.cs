using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;

internal sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    EmulationEmulatorDefinition Definition);

