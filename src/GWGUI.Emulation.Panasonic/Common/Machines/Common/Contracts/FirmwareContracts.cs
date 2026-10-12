namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(IReadOnlyDictionary<FirmwareRole, string> Images);
internal sealed record FirmwareRequirement(FirmwareRole Role, string FieldId, string DisplayResourceKey, IReadOnlyList<string> FileNames, bool IsRequired);