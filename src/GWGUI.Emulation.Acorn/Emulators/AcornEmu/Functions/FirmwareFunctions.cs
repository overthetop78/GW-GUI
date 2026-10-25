using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Charge et reconnaît les ROM externes ; les chemins sont des options persistées par le Common existant.</summary>
internal static class FirmwareFunctions
{
    /// <summary>Lit une copie possédée des octets. Le fichier source est emprunté et reste inchangé.</summary>
    internal static byte[] Read(MachineConfiguration configuration, string field)
    {
        if (!configuration.Options.TryGetValue(field, out var path) || string.IsNullOrWhiteSpace(path)) throw new FileNotFoundException(field);
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length != HardwareConstants.RomSize) throw new InvalidDataException(path);
        var role = Identify(bytes);
        if (role != field) throw new InvalidDataException(path);
        return bytes;
    }
    /// <summary>Reconnaît par contenu le couple MOS 1.20/BASIC II utilisé pour le premier jalon, indépendamment du nom.</summary>
    internal static string? Identify(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != HardwareConstants.RomSize) return null;
        return Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant() switch
        {
            "0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d" => EmulatorConstants.SystemFirmware,
            "4a7393f3a45ea309f744441c16723e2ef447a281" => EmulatorConstants.BasicFirmware,
            _ => null
        };
    }
    internal static async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanAsync(string path, CancellationToken cancellationToken)
    {
        if (new FileInfo(path).Length != HardwareConstants.RomSize) return [];
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var field = Identify(bytes);
        return field is null ? [] : [new EmulationFirmwareCandidate(field, path, field == EmulatorConstants.SystemFirmware ? "MOS 1.20" : "BASIC II", field == EmulatorConstants.SystemFirmware ? "1.20" : "II", EmulationFirmwareCompatibility.Compatible, field)];
    }
}
