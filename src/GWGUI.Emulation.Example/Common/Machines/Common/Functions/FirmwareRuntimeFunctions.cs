using System.IO;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Raccordement Common des emplacements, de l’inspection et de la préparation des firmwares du mock.</summary>
/// <remarks>Les trois adaptateurs utilisent ces mêmes opérations. Les sources restent empruntées ; seules
/// les copies du répertoire système appartiennent à la session. Aucun firmware n’est rendu obligatoire.</remarks>
internal static class FirmwareRuntimeFunctions
{
    /// <summary>Décrit les fichiers ROM et la banque facultative déclarés par le profil matériel.</summary>
    /// <param name="configuration">Configuration empruntée dont le catalogue fournit les rôles disponibles.</param>
    /// <returns>Emplacements facultatifs, avec le sous-dossier de préparation de la banque.</returns>
    internal static IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model).Firmware
        .Select(role => role == FirmwareCategory.VoiceSamples
            ? new FirmwareSlot(role.ToString(), string.Empty, VoiceSampleConstants.LabelResourceKey, SystemSubdirectory: VoiceSampleConstants.DirectoryName)
            : new FirmwareSlot(role.ToString(), role + ".rom", "Emulation.Firmware.Rom.System")).ToArray();

    /// <summary>Valide le fichier sélectionné pour le rôle demandé, en conservant la règle de taille des ROM.</summary>
    /// <param name="configuration">Profil qui déclare les rôles acceptés et la taille maximale d’une ROM.</param>
    /// <param name="fieldId">Identifiant de rôle retourné par GetFirmwareSlots.</param>
    /// <param name="path">Chemin du fichier utilisateur emprunté.</param>
    /// <returns>Compatibilité structurelle ; les erreurs d’accès ne rendent jamais un fichier compatible.</returns>
    /// <remarks>La banque doit réellement contenir les WAV attendus. Son nom n’établit pas son identité.
    /// Une archive ZIP ne prend pas la place de la ROM brute : chaque rôle conserve son traitement.</remarks>
    internal static bool IsCompatible(MachineConfiguration configuration, string fieldId, string path)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        if (!Enum.TryParse<FirmwareCategory>(fieldId, out var role) || !hardware.Firmware.Contains(role) || !File.Exists(path)) return false;
        if (role == FirmwareCategory.VoiceSamples) return VoiceSampleFunctions.IsCompatible(path);
        if (Path.GetExtension(path).Equals(VoiceSampleConstants.ArchiveExtension, StringComparison.OrdinalIgnoreCase) || VoiceSampleFunctions.IsCompatible(path)) return false;
        try
        {
            var length = new FileInfo(path).Length;
            return length > 0 && length <= hardware.MainMemoryBytes;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return false; }
    }

    /// <summary>Projette l’observation Common vers les candidats publics du gestionnaire de firmware.</summary>
    /// <param name="inspections">Adaptateurs empruntés conservant leur possibilité de compléter l’observation.</param>
    /// <param name="configuration">Profil matériel dont les rôles sont contrôlés.</param>
    /// <param name="path">Fichier candidat emprunté.</param>
    /// <param name="cancellationToken">Annulation transmise au calcul d’empreinte et au scan Common.</param>
    /// <returns>Candidat affecté au rôle reconnu ; une ROM brute sans identité reste partiellement compatible.</returns>
    internal static async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(IReadOnlyList<IEmulatorFirmwareAdapter> inspections, MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        var scanned = await FirmwareScanFunctions.ScanFileAsync(inspections, path, configuration.Model, null, cancellationToken).ConfigureAwait(false);
        var role = scanned.Definition?.Category ?? FirmwareCategory.OperatingSystem;
        var compatibility = scanned.Detection == FirmwareDetectionStatus.Unreadable || !IsCompatible(configuration, role.ToString(), path)
            ? FirmwareCompatibility.Incompatible : role == FirmwareCategory.VoiceSamples ? FirmwareCompatibility.Compatible : FirmwareCompatibility.PartiallyCompatible;
        return [new(scanned.Md5 ?? scanned.Path, scanned.Path, Path.GetFileName(scanned.Path), scanned.Version ?? scanned.Definition?.Version, compatibility, role.ToString())];
    }

    /// <summary>Prépare les sources sélectionnées dans le seul répertoire système fourni par la session.</summary>
    /// <param name="configuration">Configuration empruntée contenant les fichiers choisis.</param>
    /// <param name="systemDirectory">Répertoire de sortie possédé et nettoyé par l’appelant.</param>
    /// <remarks>Les ROM conservent leur nom de destination et leur copie existante. La banque est validée
    /// puis extraite dans son sous-dossier ; les sources utilisateur sont conservées. Toute erreur remonte
    /// au propriétaire de la création de session, qui libère son cœur et son répertoire dans finally.</remarks>
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        Directory.CreateDirectory(systemDirectory);
        foreach (var firmware in configuration.Firmwares)
        {
            if (!IsCompatible(configuration, firmware.Category.ToString(), firmware.Path)) throw new ArgumentException(nameof(configuration));
            if (firmware.Category == FirmwareCategory.VoiceSamples) { VoiceSampleFunctions.Prepare(firmware.Path, systemDirectory); continue; }
            var destination = Path.Combine(systemDirectory, firmware.Category + ".rom");
            if (!string.Equals(Path.GetFullPath(firmware.Path), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) File.Copy(firmware.Path, destination, overwrite: true);
        }
    }
}
