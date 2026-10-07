namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

internal sealed record FirmwareSlot(
    string FieldId,
    string ResourceKey,
    bool IsRequired = false,
    string? ExplanationResourceKey = null,
    string? DetailedExplanationResourceKey = null);
