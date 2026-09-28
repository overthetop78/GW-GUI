using GWGUI.Emulation;

namespace GWGUI.Emulation.Sega.Emulators.Yabause.Contracts;

internal sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    EmulationEmulatorDefinition Definition);

