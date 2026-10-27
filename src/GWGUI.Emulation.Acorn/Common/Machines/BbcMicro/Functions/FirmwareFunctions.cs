using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

/// <summary>Reconnaissance et formats de ROM BBC partagés par les émulateurs ; les fichiers utilisateur restent empruntés.</summary>
internal static class BbcMicroFirmwareFunctions
{
    /// <summary>Inspecte selon le modèle ; le backend fournit seulement la projection d'un rôle vers son champ de chargement.</summary>
    internal static async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanAsync(MachineModel model, string path, Func<FirmwareCategory, string?> destination, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (new FileInfo(path).Length > BbcMicroFirmwareConstants.BankSize * 2)
            return [new(path, path, Path.GetFileName(path), null, EmulationFirmwareCompatibility.Unknown)];
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Inspect(bytes, path, model, destination);
    }

    /// <summary>Conserve identité et compatibilité matérielle même si le cœur n'expose aucun emplacement externe.</summary>
    /// <remarks>Un rôle ou une taille plausibles n'identifient pas une version : les fichiers non reconnus restent Unknown.
    /// Une image sans rôle déterminé conserve les choix matériels possibles, sans installation ni sélection automatique.</remarks>
    internal static IReadOnlyList<EmulationFirmwareCandidate> Inspect(ReadOnlySpan<byte> bytes, string path, MachineModel model, Func<FirmwareCategory, string?> destination)
    {
        if (!BbcMicroModelCatalog.All.Any(item => item.Model == model)) throw new ArgumentOutOfRangeException(nameof(model));
        var hash = Convert.ToHexString(SHA1.HashData(bytes));
        if (FirmwareCatalog.BySha1.TryGetValue(hash, out var known))
        {
            var compatibility = BbcMicroFirmwareCompatibilityCatalog.Get(model, hash);
            if (known.Combined) return [Candidate(FirmwareCategory.OperatingSystem, known.Name, known.Version, compatibility), Candidate(FirmwareCategory.Basic, known.Name, known.Version, compatibility)];
            return [Candidate(known.Category, known.Name, known.Version, compatibility)];
        }
        var name = Path.GetFileName(path);
        if (bytes.Length == BbcMicroFirmwareConstants.CharacterRomSize) return [Candidate(FirmwareCategory.CharacterGenerator, name, null, EmulationFirmwareCompatibility.Unknown)];
        if (bytes.Length is not (BbcMicroFirmwareConstants.BankSize / 2 or BbcMicroFirmwareConstants.BankSize or BbcMicroFirmwareConstants.BankSize * 2))
            return [new(hash, path, name, null, EmulationFirmwareCompatibility.Unknown)];
        var role = Identify(bytes);
        if (role.HasValue) return [Candidate(role.Value, name, null, EmulationFirmwareCompatibility.Unknown)];
        if (bytes.Length == BbcMicroFirmwareConstants.BankSize / 2) return [Candidate(FirmwareCategory.Basic, name, null, EmulationFirmwareCompatibility.Unknown)];
        return [Candidate(FirmwareCategory.OperatingSystem, name, null, EmulationFirmwareCompatibility.Unknown), Candidate(FirmwareCategory.Basic, name, null, EmulationFirmwareCompatibility.Unknown)];

        EmulationFirmwareCandidate Candidate(FirmwareCategory role, string name, string? version, EmulationFirmwareCompatibility compatibility) => new(hash + ":" + role, path, name, version, compatibility, destination(role));
    }

    /// <summary>Reconnaît un rôle par empreinte ou structure ; le rôle ne constitue pas une validation de compatibilité.</summary>
    internal static FirmwareCategory? Identify(ReadOnlySpan<byte> bytes)
    {
        if (FirmwareCatalog.BySha1.TryGetValue(Convert.ToHexString(SHA1.HashData(bytes)), out var known)) return known.Combined ? null : known.Category;
        if (bytes.Length is not (BbcMicroFirmwareConstants.BankSize or BbcMicroFirmwareConstants.BankSize / 2)) return null;
        var copyright = bytes[7];
        if (copyright + 3 < bytes.Length && bytes[copyright] == 0 && bytes[copyright + 1] == '(' && bytes[copyright + 2] == 'C' && bytes[copyright + 3] == ')')
        {
            if ((bytes[6] & 0x40) != 0) return FirmwareCategory.Language;
            if ((bytes[6] & 0x80) != 0) return FirmwareCategory.Application;
        }
        if (bytes.Length == BbcMicroFirmwareConstants.BankSize && (bytes[^4] | bytes[^3] << 8) is >= 0xc000 and < 0xfc00) return FirmwareCategory.OperatingSystem;
        return null;
    }

    /// <summary>Prépare en mémoire une banque choisie ; l'empreinte n'est pas une liste blanche de chargement.</summary>
    internal static byte[] GetRomBytes(ReadOnlySpan<byte> bytes, FirmwareCategory role)
    {
        if (role == FirmwareCategory.CharacterGenerator)
            return bytes.Length == BbcMicroFirmwareConstants.CharacterRomSize ? bytes.ToArray() : throw new InvalidDataException(role.ToString());
        if (role is not (FirmwareCategory.OperatingSystem or FirmwareCategory.Basic or FirmwareCategory.Language or FirmwareCategory.DiscFilingSystem or FirmwareCategory.Network or FirmwareCategory.MemoryExtension)) throw new ArgumentOutOfRangeException(nameof(role));
        if (bytes.Length == BbcMicroFirmwareConstants.BankSize) return bytes.ToArray();
        if (bytes.Length == BbcMicroFirmwareConstants.BankSize * 2 && role is not (FirmwareCategory.DiscFilingSystem or FirmwareCategory.Network or FirmwareCategory.MemoryExtension)) return bytes.Slice(role == FirmwareCategory.OperatingSystem ? BbcMicroFirmwareConstants.BankSize : 0, BbcMicroFirmwareConstants.BankSize).ToArray();
        if (bytes.Length == BbcMicroFirmwareConstants.BankSize / 2 && role != FirmwareCategory.OperatingSystem)
        {
            var image = new byte[BbcMicroFirmwareConstants.BankSize];
            Array.Fill(image, (byte)255);
            bytes.CopyTo(image);
            return image;
        }
        throw new InvalidDataException(role.ToString());
    }
}
