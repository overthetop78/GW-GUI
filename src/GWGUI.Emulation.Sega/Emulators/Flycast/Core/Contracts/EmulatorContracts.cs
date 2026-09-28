using GWGUI.Emulation;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Contracts;

internal sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    EmulationEmulatorDefinition Definition);

