using System.IO;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;

internal static class FirmwareFunctions
{
    internal static IReadOnlyList<FirmwareSlot> Slots(MachineConfiguration configuration) =>
        CoreCatalog.Get(configuration.EmulatorId).Firmware.Where(slot => slot.MachineIds.Contains(configuration.Model)).ToArray();

    internal static IReadOnlyDictionary<string, string> RuntimeOptions(MachineConfiguration configuration, string systemDirectory) =>
        CoreSettingsFunctions.Configure(configuration).Options!
            .Where(option => CoreCatalog.Get(configuration.EmulatorId).Options.Any(definition => definition.Key == option.Key))
            .ToDictionary(option => option.Key, option => option.Value, StringComparer.Ordinal);

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        CoreCatalog.Get(configuration.EmulatorId).PrepareSystem?.Invoke(configuration, systemDirectory);
        foreach (var slot in Slots(configuration))
            if (configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId) is { } path)
            {
                var destination = Path.Combine(systemDirectory, slot.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(path, destination, true);
            }
    }
}
