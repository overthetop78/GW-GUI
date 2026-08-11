using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Lit un conteneur 86F et restitue ses pistes sous forme de cellules de bits normalisées.</summary>
public sealed class I86fReader
{
    /// <summary>Lit et valide l'en-tête, la table et les pistes présentes d'un conteneur 86F.</summary>
    /// <param name="path">Chemin du fichier 86F.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et le parcours des pistes.</param>
    /// <returns>Le conteneur 86F décodé sans reconstruction sectorielle.</returns>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">La signature, la table, une position, un nombre de bits ou une piste est invalide.</exception>
    /// <exception cref="OverflowException">Une position ou une taille dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    public async Task<I86fImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length < I86fLayout.MinimumFileLength || BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(I86fFormat.SignatureOffset, I86fFormat.SignatureLength)) != I86fFormat.Signature) throw I86fExceptions.MissingSignature(data.Length);

        var fileFlags = (I86fFileFlags)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength));
        var tableEntryCount = fileFlags.HasFlag(I86fFileFlags.TwoSided) ? I86fLayout.TwoSideTrackTableEntries : I86fLayout.TrackTableEntriesPerSide;
        var tableEnd = checked(I86fLayout.TrackTableOffset + tableEntryCount * I86fLayout.TrackTableEntrySize);
        if (data.Length < tableEnd) throw I86fExceptions.IncompleteTrackTable(tableEnd, data.Length);

        var tracks = new List<I86fTrack>();
        for (var logicalTrack = 0; logicalTrack < tableEntryCount; logicalTrack++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var offset = ReadOffset(data, logicalTrack);
            if (offset == 0) continue;
            var nextOffset = NextOffset(data, logicalTrack + 1, tableEntryCount, data.Length);
            var track = ReadTrack(data, logicalTrack, offset, nextOffset, fileFlags);
            if (track is not null) tracks.Add(track);
        }
        return new(fileFlags, tracks);
    }

    /// <summary>Lit une piste, normalise l'ordre des octets de ses mots et ignore une piste sans transition.</summary>
    /// <param name="data">Octets complets du conteneur.</param>
    /// <param name="logicalTrack">Index logique de la piste.</param>
    /// <param name="offset">Position du début de la piste.</param>
    /// <param name="nextOffset">Position de la prochaine piste ou fin du fichier.</param>
    /// <param name="fileFlags">Drapeaux du fichier qui déterminent la disposition de la piste.</param>
    /// <returns>La piste normalisée, ou <see langword="null"/> si elle ne contient aucune transition.</returns>
    /// <exception cref="InvalidDataException">La plage, le nombre de bits ou la charge utile de la piste est invalide.</exception>
    /// <exception cref="OverflowException">La taille alignée de la piste dépasse la capacité d'un entier.</exception>
    private static I86fTrack? ReadTrack(byte[] data, int logicalTrack, int offset, int nextOffset, I86fFileFlags fileFlags)
    {
        var hasExtraBitCells = fileFlags.HasFlag(I86fFileFlags.ExtraBitCellCount);
        var headerSize = hasExtraBitCells ? I86fLayout.ExtendedTrackHeaderSize : I86fLayout.StandardTrackHeaderSize;
        if (offset < 0 || offset > data.Length - headerSize || nextOffset < offset + headerSize) throw I86fExceptions.InvalidTrackRange(logicalTrack, offset, nextOffset, headerSize, data.Length);

        var trackFlags = (I86fTrackFlags)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset + I86fLayout.TrackFlagsOffset, I86fLayout.FileFlagsLength));
        var hasExplicitBitCount = hasExtraBitCells && (fileFlags & I86fFileFlags.SpeedShiftMask) == I86fFileFlags.None && fileFlags.HasFlag(I86fFileFlags.SpeedupOrExplicitBitCount);
        var bitCount = hasExplicitBitCount ? BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset + I86fLayout.ExplicitBitCountOffset)) : checked((nextOffset - offset - headerSize) * I86fLayout.BitsPerByte);
        if (bitCount <= 0) throw I86fExceptions.InvalidBitCount(logicalTrack, bitCount);
        var byteCount = checked(((bitCount + I86fLayout.WordBitAlignment - 1) / I86fLayout.WordBitAlignment) * I86fLayout.BytesPerWord);
        if (offset + headerSize > data.Length - byteCount) throw I86fExceptions.TruncatedTrack(logicalTrack, offset, nextOffset, byteCount, Math.Max(0, data.Length - offset - headerSize));

        var source = data.AsSpan(offset + headerSize, byteCount);
        var reverseBytes = fileFlags.HasFlag(I86fFileFlags.ReverseByteOrder);
        var bits = new bool[bitCount];
        for (var bit = 0; bit < bitCount; bit++)
        {
            var wordByte = bit / I86fLayout.WordBitAlignment * I86fLayout.BytesPerWord;
            var byteInWord = bit / I86fLayout.BitsPerByte % I86fLayout.BytesPerWord;
            if (reverseBytes) byteInWord ^= I86fLayout.BytesPerWord - 1;
            bits[bit] = (source[wordByte + byteInWord] & (I86fLayout.MostSignificantBitMask >> (bit % I86fLayout.BitsPerByte))) != 0;
        }
        return bits.Any(value => value) ? new(logicalTrack, trackFlags, bitCount, bits) : null;
    }

    /// <summary>Recherche la position de la prochaine piste présente dans la table.</summary>
    /// <param name="data">Octets complets du conteneur.</param>
    /// <param name="start">Première entrée de table à examiner.</param>
    /// <param name="count">Nombre total d'entrées de table.</param>
    /// <param name="fallback">Position retournée lorsqu'aucune piste suivante n'est présente.</param>
    /// <returns>La position de la prochaine piste ou la position de repli.</returns>
    /// <exception cref="InvalidDataException">Une position ne peut pas être représentée par le moteur.</exception>
    private static int NextOffset(byte[] data, int start, int count, int fallback)
    {
        for (var index = start; index < count; index++)
        {
            var value = ReadOffset(data, index);
            if (value != 0) return value;
        }
        return fallback;
    }

    /// <summary>Lit une position de piste et vérifie qu'elle est représentable par le moteur.</summary>
    /// <param name="data">Octets complets du conteneur.</param>
    /// <param name="logicalTrack">Index de l'entrée à lire.</param>
    /// <returns>La position de piste convertie en entier signé.</returns>
    /// <exception cref="InvalidDataException">La position dépasse la capacité d'un entier signé.</exception>
    private static int ReadOffset(byte[] data, int logicalTrack)
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(I86fLayout.TrackTableOffset + logicalTrack * I86fLayout.TrackTableEntrySize, I86fLayout.TrackTableEntrySize));
        return value <= int.MaxValue ? (int)value : throw I86fExceptions.TrackOffsetOutsideRange(logicalTrack, value, data.Length);
    }
}
