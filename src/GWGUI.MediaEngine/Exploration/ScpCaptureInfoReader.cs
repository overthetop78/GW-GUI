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
    /// <exception cref="ArgumentNullException"><paramref name="path"/> est nul.</exception>
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
            var entryOffset = ScpFormatConstants.TrackTableOffset + slot * ScpFormatConstants.TrackTableEntrySize;
            var trackOffset = BinaryPrimitives.ReadUInt32LittleEndian(table.AsSpan(entryOffset, ScpFormatConstants.TrackTableEntrySize));
            if (trackOffset != ScpFormatConstants.MissingTrackOffset) slots.Add(slot);
        }

        stream.Position = ScpFormatConstants.TrackTableOffset;
        var buffer = new byte[ReadBufferSize];
        var checksum = await ReadChecksumAsync(stream, buffer, cancellationToken).ConfigureAwait(false);
        var checksumValid = ScpFormatAlgorithms.IsChecksumValid(header.Checksum, header.Flags, checksum);
        var addresses = slots.Select(ScpFormatAlgorithms.ToTrackAddress).ToArray();
        var capturedTracks = slots.Count;
        var missingTracks = Math.Max(0, header.TrackCount - capturedTracks);
        var cylinders = addresses.Select(address => address.Cylinder).Distinct().Count();
        var sides = addresses.Select(address => address.Head).Distinct().Count();
        return new(header, capturedTracks, missingTracks, cylinders, sides, checksumValid, stream.Length);
    }

    /// <summary>Calcule incrémentalement la somme des octets lus depuis la position courante du flux jusqu'à sa fin.</summary>
    /// <param name="stream">Flux SCP positionné au début de la plage couverte par la somme.</param>
    /// <param name="buffer">Tampon réutilisé pour chaque lecture séquentielle.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler les lectures.</param>
    /// <returns>Somme non signée des octets lus.</returns>
    private static async Task<uint> ReadChecksumAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var checksum = ScpFormatConstants.InitialChecksum;
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0) return checksum;
            checksum = ScpFormatAlgorithms.UpdateChecksum(checksum, buffer.Span[..read]);
        }
    }
}
