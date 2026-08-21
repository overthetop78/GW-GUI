namespace GWGUI.Emulation.Atari;

public sealed record AtariHardwareView(
    IReadOnlyList<AtariHardwareField> Cpu,
    IReadOnlyList<AtariHardwareField> Memory,
    IReadOnlyList<AtariFirmwareDefinition> Firmware,
    IReadOnlyList<AtariHardwareChoice> Regions);
