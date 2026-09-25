namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class FirmwareSelectionFunctions
{
    internal static bool IsSystemRom(MachineModel model, FirmwareCategory category) => model switch
    {
        MachineModel.Atari400 => category == FirmwareCategory.AtariSystemOs,
        MachineModel.Atari800 => category is FirmwareCategory.AtariOsA or FirmwareCategory.AtariOsB,
        MachineModel.Atari800Xl or MachineModel.Atari130Xe or MachineModel.XlXe or
            MachineModel.Xegs => category == FirmwareCategory.AtariXlOs,
        MachineModel.Atari5200 => category == FirmwareCategory.Atari5200Bios,
        MachineModel.Atari7800 => category == FirmwareCategory.Atari7800Bios,
        MachineModel.Lynx => category == FirmwareCategory.LynxBootRom,
        MachineModel.JaguarCd => category == FirmwareCategory.JaguarCdBios,
        _ => false
    };

    internal static string? FieldId(MachineModel model, FirmwareCategory category)
    {
        if (IsSystemRom(model, category)) return SettingsConstants.SystemFirmware;
        return category switch
        {
            FirmwareCategory.AtariBasic => SettingsConstants.BasicFirmware,
            FirmwareCategory.AtariXegsBios => SettingsConstants.XegsFirmware,
            FirmwareCategory.Tos => SettingsConstants.SystemFirmware,
            _ => null
        };
    }

    internal static IReadOnlyList<FirmwareConfiguration> ReplaceField(MachineModel model,
        IEnumerable<FirmwareConfiguration> firmwares, string fieldId,
        FirmwareConfiguration? selected)
    {
        bool Matches(FirmwareConfiguration item) =>
            string.Equals(FieldId(model, item.Category), fieldId, StringComparison.Ordinal);
        return firmwares.Where(item => !Matches(item))
            .Concat(selected is null ? [] : [selected])
            .ToArray();
    }
}
