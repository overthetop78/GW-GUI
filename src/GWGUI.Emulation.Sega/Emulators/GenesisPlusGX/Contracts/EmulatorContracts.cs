using GWGUI.Emulation;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;

internal sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    EmulationEmulatorDefinition Definition);

