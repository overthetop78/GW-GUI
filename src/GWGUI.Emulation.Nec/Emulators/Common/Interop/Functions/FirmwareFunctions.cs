using System.IO;
using Quasi = GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;
using Neko = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;
using Kai = GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static class FirmwareFunctions
{
    internal static IReadOnlyList<FirmwareSlot> Slots(string emulatorId) => emulatorId switch
    {
        Quasi.CoreConstants.Id => Quasi.FirmwareConstants.Slots,
        Neko.CoreConstants.Id => Neko.FirmwareConstants.Slots,
        Kai.CoreConstants.Id => Kai.FirmwareConstants.Slots,
        _ => []
    };

    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration) =>
        Slots(configuration.EmulatorId).Select(slot => new EmulationSettingsField(slot.FieldId,
            EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
            slot.LabelResourceKey, EmulationSettingsEditor.Path,
            configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware)).ToArray();

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        foreach (var slot in Slots(configuration.EmulatorId))
            if (configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId) is { } path)
            {
                var directory = Path.Combine(systemDirectory, slot.SystemSubdirectory);
                Directory.CreateDirectory(directory);
                File.Copy(path, Path.Combine(directory, slot.FileName), true);
            }
    }
}
