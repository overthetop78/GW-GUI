using System.Buffers.Binary;
using GWGUI.MediaEngine;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Lit et valide un conteneur SCP, puis reconstruit ses pistes et leurs révolutions de flux.
/// </summary>
public sealed class ScpReader : IScpReader
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<FileIdentity, Lazy<Task<ScpImage>>> _cache = new();

    /// <summary>
    /// Lit un fichier SCP et réutilise le résultat déjà chargé tant que son chemin, sa taille et sa date de modification restent identiques.
    /// </summary>
    /// <param name="path">Chemin du fichier SCP à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler l'attente de la lecture.</param>
    /// <returns>Contenu SCP validé, avec ses pistes, ses révolutions et l'état de sa somme de contrôle.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> est vide ou n'est pas un chemin valide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier désigné par <paramref name="path"/> n'existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès en lecture au fichier est refusé.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">Le contenu ne respecte pas la structure du format SCP.</exception>
    /// <exception cref="NotSupportedException">Le conteneur utilise une variante SCP non prise en charge.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> demande l'annulation de l'attente.</exception>
    public async Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var file = new FileInfo(path);
        var identity = new FileIdentity(file.FullName, file.Length, file.LastWriteTimeUtc.Ticks);
        foreach (var obsolete in _cache.Keys.Where(key => key.Path.Equals(identity.Path, StringComparison.OrdinalIgnoreCase) && key != identity))
            _cache.TryRemove(obsolete, out _);
        var pending = _cache.GetOrAdd(identity, key => new(() => ReadFileAsync(key.Path), LazyThreadSafetyMode.ExecutionAndPublication));
        try { return await pending.Value.WaitAsync(cancellationToken).ConfigureAwait(false); }
        catch
        {
            _cache.TryRemove(identity, out _);
            throw;
        }
    }

    /// <summary>
    /// Charge tous les octets d'un fichier avant de les transmettre au lecteur de conteneur en mémoire.
    /// </summary>
    /// <param name="path">Chemin du fichier SCP à charger.</param>
    /// <returns>Contenu SCP validé et interprété.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> est vide ou n'est pas un chemin valide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier désigné par <paramref name="path"/> n'existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès en lecture au fichier est refusé.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">Le contenu ne respecte pas la structure du format SCP.</exception>
    /// <exception cref="NotSupportedException">Le conteneur utilise une variante SCP non prise en charge.</exception>
    private async Task<ScpImage> ReadFileAsync(string path)
    {
        var data = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
        return Read(data);
    }

    /// <summary>
    /// Identifie une version précise d'un fichier afin d'invalider une entrée de cache devenue obsolète.
    /// </summary>
    /// <param name="Path">Chemin complet normalisé du fichier.</param>
    /// <param name="Length">Taille du fichier, en octets.</param>
    /// <param name="LastWriteTicks">Date de dernière modification UTC, exprimée en graduations de <see cref="DateTime"/>.</param>
    private readonly record struct FileIdentity(string Path, long Length, long LastWriteTicks);

    /// <summary>
    /// Interprète un conteneur SCP déjà chargé en mémoire.
    /// </summary>
    /// <param name="data">Octets complets du conteneur SCP.</param>
    /// <returns>Image SCP contenant l'en-tête validé, les pistes présentes et l'état de la somme de contrôle.</returns>
    /// <exception cref="InvalidDataException">Une signature, une plage, une table, une piste ou une révolution SCP est invalide ou incomplète.</exception>
    /// <exception cref="NotSupportedException">Le conteneur est étendu ou déclare une largeur de cellule de bit non prise en charge.</exception>
    /// <exception cref="OverflowException">Une taille ou une position déclarée ne peut pas être représentée sans dépassement.</exception>
    public ScpImage Read(ReadOnlySpan<byte> data)
    {
        var header = ReadHeader(data);
        if ((header.Flags & ScpFlags.Extended) != 0) throw ScpExceptions.ExtendedMedia();
        var tableBytes = checked(ScpFormatConstants.TrackTableOffset + ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize);
        Require(data, 0, tableBytes, "track-offset table");
        var tracks = new List<ScpTrack>();
        for (var slot = header.StartTrack; slot <= header.EndTrack; slot++)
        {
            var offset = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(
                ScpFormatConstants.TrackTableOffset + slot * ScpFormatConstants.TrackTableEntrySize,
                ScpFormatConstants.TrackTableEntrySize));
            if (offset == 0) continue;
            tracks.Add(ReadTrack(data, checked((int)offset), slot, header));
        }
        var checksumValid = header.Checksum == 0 && (header.Flags & ScpFlags.Writable) != 0 || ComputeChecksum(data[ScpFormatConstants.TrackTableOffset..]) == header.Checksum;
        return new ScpImage(header, tracks, checksumValid, data.Length);
    }

    /// <summary>
    /// Valide et interprète les seize octets de l'en-tête fixe SCP.
    /// </summary>
    /// <param name="data">Données commençant au premier octet du conteneur SCP.</param>
    /// <returns>En-tête SCP interprété.</returns>
    /// <exception cref="InvalidDataException">L'en-tête est incomplet ou contient une signature, un nombre de révolutions, une plage de pistes ou un sélecteur de faces invalide.</exception>
    /// <exception cref="NotSupportedException">La largeur de cellule de bit déclarée n'est pas prise en charge.</exception>
    public static ScpHeader ReadHeader(ReadOnlySpan<byte> data)
    {
        Require(data, 0, ScpFormatConstants.HeaderLength, "SCP header");
        if (!data[..ScpFormatConstants.SignatureLength].SequenceEqual(ScpFormatConstants.FileSignature)) throw ScpExceptions.MissingFileSignature();
        if (data[ScpFormatConstants.RevolutionCountOffset] is < ScpFormatConstants.MinimumRevolutionCount or > ScpFormatConstants.MaximumRevolutionCount) throw ScpExceptions.InvalidRevolutionCount(data[ScpFormatConstants.RevolutionCountOffset]);
        if (data[ScpFormatConstants.EndTrackOffset] < data[ScpFormatConstants.StartTrackOffset] || data[ScpFormatConstants.EndTrackOffset] >= ScpFormatConstants.FloppyTrackSlots) throw ScpExceptions.InvalidTrackRange(data[ScpFormatConstants.StartTrackOffset], data[ScpFormatConstants.EndTrackOffset]);
        if (data[ScpFormatConstants.BitCellWidthOffset] is not (ScpFormatConstants.StandardBitCellWidth or ScpFormatConstants.AlternateBitCellWidth)) throw ScpExceptions.UnsupportedBitCellWidth(data[ScpFormatConstants.BitCellWidthOffset]);
        if (data[ScpFormatConstants.HeadsOffset] > ScpFormatConstants.MaximumHeadSelector) throw ScpExceptions.InvalidHeadSelector(data[ScpFormatConstants.HeadsOffset]);
        return new(
            data[ScpFormatConstants.VersionOffset],
            data[ScpFormatConstants.DiskTypeOffset],
            data[ScpFormatConstants.RevolutionCountOffset],
            data[ScpFormatConstants.StartTrackOffset],
            data[ScpFormatConstants.EndTrackOffset],
            (ScpFlags)data[ScpFormatConstants.FlagsOffset],
            data[ScpFormatConstants.BitCellWidthOffset],
            data[ScpFormatConstants.HeadsOffset],
            data[ScpFormatConstants.ResolutionOffset],
            BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength)));
    }

    /// <summary>
    /// Lit une piste SCP, valide ses descripteurs et convertit ses mots de flux en intervalles temporels.
    /// </summary>
    /// <param name="data">Octets complets du conteneur SCP.</param>
    /// <param name="offset">Position de la piste, en octets depuis le début du conteneur.</param>
    /// <param name="expectedTrack">Numéro d'entrée attendu d'après la table des pistes.</param>
    /// <param name="header">En-tête SCP validé qui fixe le nombre attendu de révolutions.</param>
    /// <returns>Piste interprétée ; chaque intervalle de flux est exprimé en pas temporels SCP.</returns>
    /// <exception cref="InvalidDataException">La piste, un descripteur ou les données de flux sont incomplets ou incohérents.</exception>
    /// <exception cref="OverflowException">Une taille, une position ou un intervalle de flux dépasse la plage numérique prise en charge.</exception>
    private static ScpTrack ReadTrack(ReadOnlySpan<byte> data, int offset, int expectedTrack, ScpHeader header)
    {
        var descriptorSize = checked(ScpFormatConstants.TrackDescriptorHeaderSize + header.Revolutions * ScpFormatConstants.RevolutionDescriptorSize);
        Require(data, offset, descriptorSize, $"track {expectedTrack} header");
        var trackData = data[offset..];
        if (!trackData[..ScpFormatConstants.SignatureLength].SequenceEqual(ScpFormatConstants.TrackSignature)) throw ScpExceptions.MissingTrackSignature(expectedTrack, trackData[ScpFormatConstants.TrackNumberOffset]);
        if (trackData[ScpFormatConstants.TrackNumberOffset] != expectedTrack) throw ScpExceptions.TrackNumberMismatch(expectedTrack, trackData[ScpFormatConstants.TrackNumberOffset]);
        var revolutions = new List<ScpRevolution>(header.Revolutions);
        for (var index = 0; index < header.Revolutions; index++)
        {
            var descriptor = ScpFormatConstants.TrackDescriptorHeaderSize + index * ScpFormatConstants.RevolutionDescriptorSize;
            var indexTime = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptor + ScpFormatConstants.RevolutionIndexTimeOffset, sizeof(uint)));
            var fluxCount = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptor + ScpFormatConstants.RevolutionFluxCountOffset, sizeof(uint)));
            var relativeOffset = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptor + ScpFormatConstants.RevolutionDataOffset, sizeof(uint)));
            var byteCount = checked((int)fluxCount * ScpFormatConstants.FluxIntervalSize);
            Require(data, checked(offset + (int)relativeOffset), byteCount, $"track {expectedTrack}, revolution {index + 1} flux");
            var fluxBytes = data.Slice(offset + (int)relativeOffset, byteCount);
            var intervals = new List<uint>((int)Math.Min(fluxCount, (uint)int.MaxValue));
            uint overflow = 0;
            for (var position = 0; position < fluxBytes.Length; position += ScpFormatConstants.FluxIntervalSize)
            {
                var value = BinaryPrimitives.ReadUInt16BigEndian(fluxBytes.Slice(position, ScpFormatConstants.FluxIntervalSize));
                if (value == 0) { overflow = checked(overflow + ScpFormatConstants.ZeroFluxIntervalOverflow); continue; }
                intervals.Add(checked(overflow + value));
                overflow = 0;
            }
            if (overflow != 0) intervals.Add(overflow);
            revolutions.Add(new(indexTime, fluxCount, intervals));
        }
        return new((byte)expectedTrack, expectedTrack / 2, expectedTrack % 2, revolutions);
    }

    /// <summary>
    /// Additionne les octets couverts par la somme de contrôle SCP avec un cumul non signé sur 32 bits.
    /// </summary>
    /// <param name="data">Octets à additionner.</param>
    /// <returns>Somme des octets modulo 2<sup>32</sup>.</returns>
    private static uint ComputeChecksum(ReadOnlySpan<byte> data)
    {
        uint sum = 0;
        foreach (var value in data) sum = unchecked(sum + value);
        return sum;
    }

    /// <summary>
    /// Vérifie qu'une section annoncée appartient entièrement aux données disponibles.
    /// </summary>
    /// <param name="data">Données complètes dans lesquelles la section doit se trouver.</param>
    /// <param name="offset">Position de début de la section, en octets.</param>
    /// <param name="length">Longueur requise de la section, en octets.</param>
    /// <param name="section">Nom technique de la section utilisé dans le message d'erreur.</param>
    /// <exception cref="InvalidDataException">La position ou la longueur est négative, ou la section dépasse les données disponibles.</exception>
    private static void Require(ReadOnlySpan<byte> data, int offset, int length, string section)
    {
        if (offset < 0 || length < 0 || offset > data.Length - length)
            throw ScpExceptions.IncompleteSection(section, offset, length);
    }
}
