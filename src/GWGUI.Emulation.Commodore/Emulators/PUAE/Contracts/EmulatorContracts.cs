using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Contracts;

internal sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    EmulationEmulatorDefinition Definition);
