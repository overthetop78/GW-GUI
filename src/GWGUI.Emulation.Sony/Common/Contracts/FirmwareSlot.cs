namespace GWGUI.Emulation.Sony.Common.Contracts;

internal sealed record FirmwareSlot(string FieldId, string FileName, string LabelResourceKey,
    bool IsRequired, string? DisplayName = null);
