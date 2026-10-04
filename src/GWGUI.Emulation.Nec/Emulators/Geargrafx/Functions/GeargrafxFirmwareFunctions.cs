using System.IO;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

internal static class GeargrafxFirmwareFunctions
{
    internal static void Prepare(string firmwareDirectory, string sessionDirectory,
        string? selectedFirmwarePath)
    {
        if (!string.IsNullOrWhiteSpace(selectedFirmwarePath)
            && !File.Exists(selectedFirmwarePath))
            throw new FileNotFoundException(null, selectedFirmwarePath);
        var selectedName = string.IsNullOrWhiteSpace(selectedFirmwarePath)
            ? null : Path.GetFileName(selectedFirmwarePath);
        var preferPceLp1 = string.Equals(selectedName,
            GeargrafxFirmwareConstants.PceLp1, StringComparison.OrdinalIgnoreCase);
        var systemDirectory = Path.Combine(sessionDirectory,
            CoreDirectoryConstants.SystemDirectoryName);
        Directory.CreateDirectory(systemDirectory);

        if (Directory.Exists(firmwareDirectory))
            foreach (var source in Directory.EnumerateFiles(firmwareDirectory,
                         FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var name = Path.GetFileName(source);
                if (!GeargrafxFirmwareConstants.SupportedNames.Contains(name)) continue;
                if (preferPceLp1 && name.Equals(GeargrafxFirmwareConstants.PacN1,
                        StringComparison.OrdinalIgnoreCase)) continue;
                var destination = Path.Combine(systemDirectory, name);
                if (!File.Exists(destination)) File.Copy(source, destination);
            }

        if (string.IsNullOrWhiteSpace(selectedFirmwarePath)) return;
        if (GeargrafxFirmwareConstants.SupportedNames.Contains(selectedName!))
        {
            File.Copy(selectedFirmwarePath, Path.Combine(systemDirectory, selectedName!), true);
            return;
        }
        var firmware = FirmwareCatalog.Inspect(selectedFirmwarePath);
        if (firmware.Md5.Equals(FirmwareCatalogConstants.SystemCard3Md5,
                StringComparison.OrdinalIgnoreCase))
            File.Copy(selectedFirmwarePath,
                Path.Combine(systemDirectory, GeargrafxFirmwareConstants.SystemCard3), true);
    }
}
