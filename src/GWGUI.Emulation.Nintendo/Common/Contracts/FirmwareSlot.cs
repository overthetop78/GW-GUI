namespace GWGUI.Emulation.Nintendo.Common.Contracts;

internal sealed record FirmwareSlot(string FieldId, string FileName, string LabelResourceKey,
    bool IsRequired, IReadOnlySet<string> MachineIds);
