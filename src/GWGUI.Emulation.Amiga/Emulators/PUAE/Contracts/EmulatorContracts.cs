using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;

internal sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    EmulationEmulatorDefinition Definition);
