namespace GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

internal sealed record FirmwareSlot(string FieldId, string FileName, string LabelResourceKey, string SystemSubdirectory,
    IReadOnlyList<string> Aliases, string? KnownMd5 = null);
