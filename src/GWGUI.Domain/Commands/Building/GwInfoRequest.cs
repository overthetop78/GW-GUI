namespace GWGUI.Domain.Commands.Building;

public sealed record GwInfoRequest(
    string Executable,
    string? Device = null,
    bool Bootloader = false);
