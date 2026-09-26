namespace GWGUI.Emulation.Atari.Emulators.Libretro.Contracts;

internal sealed record EnvironmentExtendedMessage(string Text, uint DurationMilliseconds, uint Priority,
    uint Level, uint Target, uint Type, sbyte Progress);

internal sealed record EnvironmentMessage(string Text, uint Frames);
