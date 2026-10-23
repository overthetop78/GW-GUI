using System.IO;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static class AmigaFirmwareFunctions
{
    internal static IReadOnlyList<EmulationFirmwareCandidate> Scan(string machineId, string directory, CancellationToken cancellationToken) =>
        new AmigaFirmwareCatalog(directory).Scan().Select(firmware =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToCandidate(firmware, machineId);
        }).ToArray();

    /// <summary>Inspecte un fichier avec le même catalogue et les mêmes destinations que le parcours du dossier.</summary>
    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(string machineId, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([ToCandidate(AmigaFirmwareCatalog.Inspect(path), machineId)]);
    }

    private static EmulationFirmwareCandidate ToCandidate(AmigaFirmware firmware, string machineId) => new(firmware.Sha256, firmware.Path,
                firmware.Name ?? Path.GetFileName(firmware.Path), firmware.Version,
                Compatibility(firmware, machineId), firmware.Type switch
                {
                    FirmwareType.Kickstart => SettingsConstants.KickstartPath,
                    FirmwareType.ExtendedRom => SettingsConstants.ExtendedRomPath,
                    FirmwareType.RomKey => SettingsConstants.RomKeyPath,
                    _ => null
                });

    private static EmulationFirmwareCompatibility Compatibility(AmigaFirmware firmware, string machineId)
    {
        if (firmware.Type == FirmwareType.Unknown) return EmulationFirmwareCompatibility.Incompatible;
        if (firmware.Type == FirmwareType.RomKey) return EmulationFirmwareCompatibility.Compatible;
        if (!firmware.CompatibleModels.Contains(machineId, StringComparer.OrdinalIgnoreCase))
            return EmulationFirmwareCompatibility.Incompatible;
        return firmware.IsOfficial ? EmulationFirmwareCompatibility.Official : firmware.IsKnown ? EmulationFirmwareCompatibility.Compatible : EmulationFirmwareCompatibility.PartiallyCompatible;
    }
}
