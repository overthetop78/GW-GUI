namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(IReadOnlyDictionary<FirmwareCategory, string> Images);
internal sealed record FirmwareRequirement(FirmwareCategory Role, string FieldId, string DisplayResourceKey, IReadOnlyList<string> FileNames, bool IsRequired);