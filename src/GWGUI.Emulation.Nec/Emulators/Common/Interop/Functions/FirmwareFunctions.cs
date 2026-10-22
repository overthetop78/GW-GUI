using System.IO;
using CommonFirmwareFunctions = GWGUI.Emulation.Nec.Common.Machines.Common.Functions.FirmwareFunctions;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static class FirmwareFunctions
{
    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration) =>
        CommonFirmwareFunctions.Slots(configuration).Select(slot => new EmulationSettingsField(slot.FieldId,
            EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
            slot.LabelResourceKey, EmulationSettingsEditor.Path,
            configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware)).ToArray();

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        foreach (var slot in CommonFirmwareFunctions.Slots(configuration))
            if (configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId) is { } path)
            {
                var directory = Path.Combine(systemDirectory, slot.SystemSubdirectory);
                Directory.CreateDirectory(directory);
                File.Copy(path, Path.Combine(directory, slot.FileName), true);
            }
    }
}
