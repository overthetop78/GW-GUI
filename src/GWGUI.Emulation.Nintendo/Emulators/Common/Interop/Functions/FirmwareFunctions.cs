using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;

internal static class FirmwareFunctions
{
    internal static IReadOnlyList<FirmwareSlot> Slots(MachineConfiguration configuration) =>
        string.IsNullOrEmpty(configuration.EmulatorId) ? [] : CoreCatalog.Get(configuration.EmulatorId).Firmware.Where(slot => slot.MachineIds.Contains(configuration.Model)).ToArray();

    internal static IReadOnlyDictionary<string, string> RuntimeOptions(MachineConfiguration configuration, string systemDirectory)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        if (configuration.EmulatorId == Emulators.MelonDsDs.Constants.CoreConstants.Id)
            foreach (var slot in Slots(configuration))
                if (configuration.FirmwarePaths?.ContainsKey(slot.FieldId) == true
                    && Emulators.MelonDsDs.Constants.FirmwareConstants.PathOptions.TryGetValue(slot.FileName, out var key))
                    options[key] = Path.GetFullPath(Path.Combine(systemDirectory, slot.FileName));
        return options;
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        foreach (var slot in Slots(configuration))
            if (configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId) is { } path)
            {
                var destination = Path.Combine(systemDirectory, slot.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(path, destination, true);
            }
    }
}
