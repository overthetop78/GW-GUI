namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariTosHeader(string Version, AtariStRegion Region, AtariTosVariant Variant,
    long ImageSize);
