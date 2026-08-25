namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariEnvironmentExtendedMessage(string Text, uint DurationMilliseconds, uint Priority,
    uint Level, uint Target, uint Type, sbyte Progress);
