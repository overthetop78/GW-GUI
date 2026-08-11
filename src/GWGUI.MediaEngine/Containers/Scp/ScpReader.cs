using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Lit et valide un conteneur SCP, puis reconstruit ses pistes et leurs révolutions de flux.</summary>
public sealed class ScpReader : IScpReader
{
    /// <summary>Conserve les images déjà analysées tant que leur fichier source reste inchangé.</summary>
    private readonly ScpFileCache _fileCache = new();

    /// <summary>Lit un fichier SCP et réutilise le résultat déjà chargé tant que son chemin, sa taille et sa date de modification restent identiques.</summary>
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
    public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => _fileCache.GetOrAddAsync(path, ReadFileAsync, cancellationToken);

    /// <summary>Charge tous les octets d'un fichier avant de les transmettre au lecteur de conteneur en mémoire.</summary>
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

    /// <summary>Interprète un conteneur SCP déjà chargé en mémoire.</summary>
    /// <param name="data">Octets complets du conteneur SCP.</param>
    /// <returns>Image SCP contenant l'en-tête validé, les pistes présentes et l'état de la somme de contrôle.</returns>
    /// <exception cref="InvalidDataException">Une signature, une plage, une table, une piste ou une révolution SCP est invalide ou incomplète.</exception>
    /// <exception cref="NotSupportedException">Le conteneur est étendu ou déclare une largeur de cellule de bit non prise en charge.</exception>
    /// <exception cref="OverflowException">Une taille ou une position déclarée ne peut pas être représentée sans dépassement.</exception>
    public ScpImage Read(ReadOnlySpan<byte> data)
    {
        var header = ReadHeader(data);
        if ((header.Flags & ScpFlags.Extended) != ScpFlags.None) throw ScpExceptions.ExtendedMedia();
        var tableBytes = checked(ScpFormatConstants.TrackTableOffset + ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize);
        ScpDataValidator.Require(data, ScpFormatConstants.FileStartOffset, tableBytes, ScpSection.TrackOffsetTable);
        var tracks = new List<ScpTrack>();
        for (var slot = header.StartTrack; slot <= header.EndTrack; slot++)
        {
            var offset = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(ScpFormatConstants.TrackTableOffset + slot * ScpFormatConstants.TrackTableEntrySize, ScpFormatConstants.TrackTableEntrySize));
            if (offset == ScpFormatConstants.MissingTrackOffset) continue;
            tracks.Add(ReadTrack(data, checked((int)offset), slot, header));
        }

        var checksum = ScpFormatAlgorithms.ComputeChecksum(data[ScpFormatConstants.TrackTableOffset..]);
        var checksumValid = ScpFormatAlgorithms.IsChecksumValid(header.Checksum, header.Flags, checksum);
        return new ScpImage(header, tracks, checksumValid, data.Length);
    }

    /// <summary>Valide et interprète les seize octets de l'en-tête fixe SCP.</summary>
    /// <param name="data">Données commençant au premier octet du conteneur SCP.</param>
    /// <returns>En-tête SCP interprété.</returns>
    /// <exception cref="InvalidDataException">L'en-tête est incomplet ou contient une signature, un nombre de révolutions, une plage de pistes ou un sélecteur de faces invalide.</exception>
    /// <exception cref="NotSupportedException">La largeur de cellule de bit déclarée n'est pas prise en charge.</exception>
    public static ScpHeader ReadHeader(ReadOnlySpan<byte> data)
    {
        ScpDataValidator.Require(data, ScpFormatConstants.FileStartOffset, ScpFormatConstants.HeaderLength, ScpSection.Header);
        if (!data[..ScpFormatConstants.SignatureLength].SequenceEqual(ScpFormatConstants.FileSignature)) throw ScpExceptions.MissingFileSignature();
        var revolutions = data[ScpFormatConstants.RevolutionCountOffset];
        var startTrack = data[ScpFormatConstants.StartTrackOffset];
        var endTrack = data[ScpFormatConstants.EndTrackOffset];
        var bitCellEncoding = (ScpBitCellEncoding)data[ScpFormatConstants.BitCellWidthOffset];
        var heads = (ScpHeadSelection)data[ScpFormatConstants.HeadsOffset];
        if (revolutions is < ScpFormatConstants.MinimumRevolutionCount or > ScpFormatConstants.MaximumRevolutionCount) throw ScpExceptions.InvalidRevolutionCount(revolutions);
        if (endTrack < startTrack || endTrack >= ScpFormatConstants.FloppyTrackSlots) throw ScpExceptions.InvalidTrackRange(startTrack, endTrack);
        if (bitCellEncoding is not (ScpBitCellEncoding.Default16Bit or ScpBitCellEncoding.Explicit16Bit)) throw ScpExceptions.UnsupportedBitCellWidth((byte)bitCellEncoding);
        if (heads is not (ScpHeadSelection.Both or ScpHeadSelection.Side0 or ScpHeadSelection.Side1)) throw ScpExceptions.InvalidHeadSelector((byte)heads);
        return new(data[ScpFormatConstants.VersionOffset], data[ScpFormatConstants.DiskTypeOffset], revolutions, startTrack, endTrack, (ScpFlags)data[ScpFormatConstants.FlagsOffset], bitCellEncoding, heads, data[ScpFormatConstants.ResolutionOffset], BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength)));
    }

    /// <summary>Lit une piste SCP, valide son en-tête et confie chaque descripteur de révolution au lecteur spécialisé.</summary>
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
        ScpDataValidator.Require(data, offset, descriptorSize, ScpSection.TrackHeader, expectedTrack);
        var trackData = data[offset..];
        if (!trackData[..ScpFormatConstants.SignatureLength].SequenceEqual(ScpFormatConstants.TrackSignature)) throw ScpExceptions.MissingTrackSignature(expectedTrack, trackData[ScpFormatConstants.TrackNumberOffset]);
        if (trackData[ScpFormatConstants.TrackNumberOffset] != expectedTrack) throw ScpExceptions.TrackNumberMismatch(expectedTrack, trackData[ScpFormatConstants.TrackNumberOffset]);
        var revolutions = new List<ScpRevolution>(header.Revolutions);
        for (var index = 0; index < header.Revolutions; index++)
        {
            revolutions.Add(ScpRevolutionReader.Read(data, offset, ScpFormatConstants.TrackDescriptorHeaderSize + index * ScpFormatConstants.RevolutionDescriptorSize, expectedTrack, index));
        }

        var address = ScpFormatAlgorithms.ToTrackAddress(expectedTrack);
        return new((byte)expectedTrack, address.Cylinder, address.Head, revolutions);
    }
}
