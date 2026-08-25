namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariMemoryExpansionChoice(
    string Value,
    long AdditionalBytes);
