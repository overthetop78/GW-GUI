namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariHardwareView(
    IReadOnlyList<AtariHardwareField> Cpu,
    IReadOnlyList<AtariHardwareField> Memory,
    IReadOnlyList<AtariFirmwareDefinition> Firmware,
    IReadOnlyList<AtariHardwareChoice> Regions);
