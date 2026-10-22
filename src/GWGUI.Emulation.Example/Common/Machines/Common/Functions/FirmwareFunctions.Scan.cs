using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Inspection en lecture seule des fichiers candidats au firmware d’un profil.</summary>
internal static class FirmwareScanFunctions
{
    /// <summary>Énumère les candidats sans conserver de handle de répertoire après le retour.</summary>
    /// <param name="directory">Dossier utilisateur de recherche, conservé sous sa responsabilité.</param>
    /// <returns>Chemins triés ; un dossier absent ne contient aucun candidat.</returns>
    internal static IReadOnlyList<string> EnumerateCandidates(string directory) => Directory.Exists(directory)
        ? Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Order(StringComparer.OrdinalIgnoreCase).ToArray() : [];

    /// <summary>Observe la taille et l’empreinte d’un candidat sans le modifier.</summary>
    /// <param name="inspections">Adaptateurs empruntés consultés dans leur ordre pour compléter les données observées.</param>
    /// <param name="path">Chemin du fichier emprunté.</param>
    /// <param name="model">Profil dont les rôles de firmware sont consultés.</param>
    /// <param name="region">Région attendue, conservée pour l’identification par l’adaptateur.</param>
    /// <param name="cancellationToken">Annulation de la lecture et du calcul d’empreinte.</param>
    /// <returns>Candidat complété par l’adaptateur lorsqu’il est reconnu, sinon observations conservées sans identité ajoutée.</returns>
    /// <remarks>Une identité n’est pas déduite du nom de fichier. Le flux possédé est libéré dans le finally
    /// de await using ; l’annulation est propagée, les erreurs d’accès deviennent un résultat Unreadable.</remarks>
    internal static async Task<ScannedFirmware> ScanFileAsync(IReadOnlyList<IEmulatorFirmwareInspection> inspections, string path, MachineModel model, HardwareRegion? region, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferConstants.FileTransferBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var length = stream.Length;
            var hash = await MD5.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            var compatibility = HardwareModelCatalog.Get(model).Firmware.Count == 0 ? FirmwareCompatibility.Incompatible : FirmwareCompatibility.PartiallyCompatible;
            var scanned = new ScannedFirmware(path, length, Convert.ToHexStringLower(hash), FirmwareDetectionStatus.Unknown, null, compatibility, false, null);
            foreach (var inspection in inspections)
                if (inspection.IdentifyFirmware(scanned) is { } recognized) return recognized;
            return scanned;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or CryptographicException)
        {
            return new(path, null, null, FirmwareDetectionStatus.Unreadable, null, FirmwareCompatibility.Incompatible, false, error.Message);
        }
    }
}
