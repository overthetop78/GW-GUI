using System.Buffers.Binary;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>
/// Lit les métadonnées finales d'une capture SCP sans décoder les données de flux des pistes.
/// </summary>
public static class ScpCaptureInfoReader
{
    /// <summary>
    /// Taille, en octets, du tampon utilisé pour les lectures séquentielles du fichier SCP.
    /// </summary>
    private const int ReadBufferSize = 81920;

    /// <summary>
    /// Lit l'en-tête, la table des pistes et la somme de contrôle d'un fichier SCP.
    /// </summary>
    /// <param name="path">Chemin du fichier SCP à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler les lectures asynchrones.</param>
    /// <returns>
    /// Les métadonnées de la capture. Les nombres de pistes, de cylindres et de faces sont des décomptes
    /// positifs ou nuls ; la taille du fichier est exprimée en octets.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> est vide ou n'est pas un chemin valide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier désigné par <paramref name="path"/> n'existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès en lecture au fichier est refusé.</exception>
    /// <exception cref="EndOfStreamException">Le fichier ne contient pas l'intégralité de l'en-tête et de la table des pistes SCP.</exception>
    /// <exception cref="InvalidDataException">L'en-tête SCP est absent, incomplet ou invalide.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> demande l'annulation de l'opération.</exception>
    public static async Task<ScpCaptureInfo> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ReadBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var tableLength = ScpFormatConstants.TrackTableOffset + ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize;
        var table = new byte[tableLength];
        await stream.ReadExactlyAsync(table, cancellationToken).ConfigureAwait(false);
        var header = ScpReader.ReadHeader(table);
        var slots = new List<int>();
        for (var slot = header.StartTrack; slot <= header.EndTrack; slot++)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(table.AsSpan(
                    ScpFormatConstants.TrackTableOffset + slot * ScpFormatConstants.TrackTableEntrySize,
                    ScpFormatConstants.TrackTableEntrySize)) != 0)
                slots.Add(slot);
        }

        stream.Position = ScpFormatConstants.TrackTableOffset;
        var buffer = new byte[ReadBufferSize];
        uint checksum = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0) break;
            for (var index = 0; index < read; index++) checksum = unchecked(checksum + buffer[index]);
        }
        var checksumValid = header.Checksum == 0 && (header.Flags & ScpFlags.Writable) != 0 || checksum == header.Checksum;
        return new(
            header,
            slots.Count,
            Math.Max(0, header.TrackCount - slots.Count),
            slots.Select(slot => slot / 2).Distinct().Count(),
            slots.Select(slot => slot % 2).Distinct().Count(),
            checksumValid,
            stream.Length);
    }
}
