namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariFirmwareSelectionFunctions
{
    internal static bool IsSystemRom(AtariMachineModel model, AtariFirmwareCategory category) => model switch
    {
        AtariMachineModel.Atari400 => category == AtariFirmwareCategory.AtariSystemOs,
        AtariMachineModel.Atari800 => category is AtariFirmwareCategory.AtariOsA or AtariFirmwareCategory.AtariOsB,
        AtariMachineModel.Atari800Xl or AtariMachineModel.Atari130Xe or AtariMachineModel.XlXe or
            AtariMachineModel.Xegs => category == AtariFirmwareCategory.AtariXlOs,
        AtariMachineModel.Atari5200 => category == AtariFirmwareCategory.Atari5200Bios,
        _ => false
    };

    internal static string? FieldId(AtariMachineModel model, AtariFirmwareCategory category)
    {
        if (IsSystemRom(model, category)) return AtariSettingsConstants.SystemFirmware;
        return category switch
        {
            AtariFirmwareCategory.AtariBasic => AtariSettingsConstants.BasicFirmware,
            AtariFirmwareCategory.AtariXegsBios => AtariSettingsConstants.XegsFirmware,
            AtariFirmwareCategory.Tos => AtariSettingsConstants.SystemFirmware,
            _ => null
        };
    }

    internal static IReadOnlyList<AtariFirmwareConfiguration> ReplaceField(AtariMachineModel model,
        IEnumerable<AtariFirmwareConfiguration> firmwares, string fieldId,
        AtariFirmwareConfiguration? selected)
    {
        bool Matches(AtariFirmwareConfiguration item) =>
            string.Equals(FieldId(model, item.Category), fieldId, StringComparison.Ordinal);
        return firmwares.Where(item => !Matches(item))
            .Concat(selected is null ? [] : [selected])
            .ToArray();
    }
}
