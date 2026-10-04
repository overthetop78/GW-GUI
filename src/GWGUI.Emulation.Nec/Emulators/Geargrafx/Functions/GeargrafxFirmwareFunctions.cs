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
        var firmware = FirmwareCatalog.Inspect(selectedFirmwarePath);
        var destinationName = firmware.Md5 switch
        {
            FirmwareCatalogConstants.SystemCard1Md5 => GeargrafxFirmwareConstants.SystemCard1,
            FirmwareCatalogConstants.SystemCard2Md5 or FirmwareCatalogConstants.SystemCard21Md5
                or FirmwareCatalogConstants.SystemCard2UsMd5 => GeargrafxFirmwareConstants.SystemCard2,
            FirmwareCatalogConstants.GamesExpressBlueMd5 or FirmwareCatalogConstants.GamesExpressGreenMd5
                => GeargrafxFirmwareConstants.GameExpress,
            FirmwareCatalogConstants.SystemCard3Md5 or FirmwareCatalogConstants.SystemCard3UsMd5
                => GeargrafxFirmwareConstants.SystemCard3,
            _ when GeargrafxFirmwareConstants.SupportedNames.Contains(selectedName!) => selectedName!,
            _ => GeargrafxFirmwareConstants.SystemCard3
        };
        File.Copy(selectedFirmwarePath, Path.Combine(systemDirectory, destinationName), true);
    }
}
