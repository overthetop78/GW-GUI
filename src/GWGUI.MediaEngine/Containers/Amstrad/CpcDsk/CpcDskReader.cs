using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>
/// Lit les conteneurs CPCEMU DSK standard et étendu et restitue leurs pistes sous forme de secteurs,
/// sans attribuer la capture à une machine CPC ou PCW.
/// </summary>
public sealed class CpcDskReader
{
    /// <summary>
    /// Lit et valide un conteneur CPCEMU DSK standard ou étendu.
    /// </summary>
    /// <param name="path">Chemin du conteneur DSK à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du fichier et le parcours des pistes.</param>
    /// <returns>
    /// Une image sectorielle neutre identifiée par <see cref="DiskImageFormatIds.CpcEmuDsk"/>. Les adresses de cylindre, de face et de secteur
    /// proviennent des descripteurs de pistes ; les tailles et la capacité sont exprimées en octets.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> est vide ou n'est pas un chemin valide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier désigné par <paramref name="path"/> n'existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès en lecture au fichier est refusé.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">
    /// La signature, la géométrie, la table des pistes, un en-tête de piste ou un secteur est absent, tronqué ou invalide,
    /// ou le conteneur ne contient aucun secteur.
    /// </exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> demande l'annulation de l'opération.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(bytes, cancellationToken);
    }

    /// <summary>Lit et valide un conteneur CPCEMU déjà chargé en mémoire.</summary>
    /// <param name="bytes">Contenu complet du conteneur.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le parcours des pistes.</param>
    /// <returns>Image sectorielle neutre reconstruite depuis le conteneur.</returns>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(Read(bytes.ToArray(), cancellationToken));

    /// <summary>Valide et reconstruit le contenu CPCEMU fourni.</summary>
    /// <param name="bytes">Contenu complet du conteneur.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le parcours des pistes.</param>
    /// <returns>Image sectorielle neutre reconstruite.</returns>
    private static SectorImage Read(byte[] bytes, CancellationToken cancellationToken)
    {
        var (extended, cylinders, heads, trackCount) = ReadDiskHeader(bytes);
        ValidateExtendedTrackSizeTable(extended, trackCount);

        var blocks = new List<SectorBlock>();
        var position = CpcDskLayout.DiskInformationBlockSize;
        var maximumSectors = 0;
        var sectorSizes = new Dictionary<int, int>();
        for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = ReadTrack(bytes, extended, trackIndex, position, blocks, sectorSizes);
            position = track.NextPosition;
            var sectorCount = track.SectorCount;
            maximumSectors = Math.Max(maximumSectors, sectorCount);
        }
        if (blocks.Count == 0) throw CpcDskExceptions.NoSectors();
        var dominantSize = sectorSizes.OrderByDescending(item => item.Value).First().Key;
        return new(CpcDskFormat.FormatId, dominantSize, cylinders, heads, Math.Max(CpcDskLayout.MinimumSectorsPerTrack, maximumSectors), blocks,
            allowVariableBlockSize: sectorSizes.Count > 1, capacity: blocks.Sum(block => (long)block.Data.Count), logicalBlockCount: blocks.Count);
    }

    /// <summary>Valide l'en-tête disque et en extrait le type de conteneur et la géométrie déclarée.</summary>
    /// <param name="bytes">Contenu du conteneur commençant par le bloc d'informations disque.</param>
    /// <returns>Type Extended, nombres de cylindres, de faces et de pistes déclarés.</returns>
    private static (bool Extended, int Cylinders, int Heads, int TrackCount) ReadDiskHeader(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CpcDskLayout.DiskInformationBlockSize) throw CpcDskExceptions.TruncatedHeader();
        var signature = bytes[..CpcDskLayout.DiskSignatureLength];
        var extended = signature.StartsWith(CpcDskFormat.ExtendedSignatureBytes);
        if (!extended && !signature.StartsWith(CpcDskFormat.StandardSignatureBytes)) throw CpcDskExceptions.UnrecognizedSignature();
        var cylinders = bytes[CpcDskLayout.CylinderCountOffset];
        var heads = bytes[CpcDskLayout.HeadCountOffset];
        if (cylinders is < CpcDskLayout.MinimumCylinderCount or > CpcDskLayout.MaximumCylinderCount || heads is < CpcDskLayout.MinimumHeadCount or > CpcDskLayout.MaximumHeadCount) throw CpcDskExceptions.InvalidGeometry();
        return (extended, cylinders, heads, checked(cylinders * heads));
    }

    /// <summary>Vérifie que la table Extended contient une entrée pour chaque piste déclarée.</summary>
    /// <param name="extended">Indique si le conteneur utilise la disposition Extended.</param>
    /// <param name="trackCount">Nombre total de pistes déclaré par la géométrie.</param>
    private static void ValidateExtendedTrackSizeTable(bool extended, int trackCount)
    {
        if (!extended || CpcDskLayout.ExtendedTrackSizeTableOffset + trackCount <= CpcDskLayout.DiskInformationBlockSize) return;
        throw CpcDskExceptions.InvalidExtendedTrackTable(CpcDskLayout.DiskInformationBlockSize - CpcDskLayout.ExtendedTrackSizeTableOffset);
    }

    /// <summary>Valide une piste, lit ses descripteurs et ajoute ses secteurs dans l'ordre de leurs identifiants.</summary>
    /// <param name="bytes">Contenu complet du conteneur.</param>
    /// <param name="extended">Indique si les tailles de pistes et de secteurs suivent la disposition Extended.</param>
    /// <param name="trackIndex">Index linéaire de la piste.</param>
    /// <param name="position">Position du bloc d'informations de piste.</param>
    /// <param name="blocks">Blocs sectoriels déjà reconstruits.</param>
    /// <param name="sectorSizes">Occurrences observées pour chaque taille sectorielle nominale.</param>
    /// <returns>Position de la piste suivante et nombre de secteurs déclaré par la piste.</returns>
    private static (int NextPosition, int SectorCount) ReadTrack(byte[] bytes, bool extended, int trackIndex, int position, List<SectorBlock> blocks, Dictionary<int, int> sectorSizes)
    {
        var trackSize = extended ? bytes[CpcDskLayout.ExtendedTrackSizeTableOffset + trackIndex] * CpcDskLayout.ExtendedTrackSizeUnit : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(CpcDskLayout.StandardTrackSizeOffset, CpcDskLayout.StoredSizeFieldLength));
        if (trackSize == 0) return (position, 0);
        if (position + trackSize > bytes.Length || trackSize < CpcDskLayout.TrackInformationBlockSize) throw CpcDskExceptions.TruncatedTrack(trackIndex);
        if (!bytes.AsSpan(position, CpcDskLayout.TrackSignatureLength).StartsWith(CpcDskFormat.TrackSignatureBytes)) throw CpcDskExceptions.InvalidTrackHeader(trackIndex);
        var cylinder = bytes[position + CpcDskLayout.TrackCylinderOffset];
        var head = bytes[position + CpcDskLayout.TrackHeadOffset];
        var sectorCount = bytes[position + CpcDskLayout.TrackSectorCountOffset];
        ReadSectorDescriptors(bytes, extended, trackIndex, position, trackSize, cylinder, head, sectorCount, blocks, sectorSizes);
        return (position + trackSize, sectorCount);
    }

    /// <summary>Valide les descripteurs d'une piste et reconstruit leurs données sectorielles.</summary>
    /// <param name="bytes">Contenu complet du conteneur.</param>
    /// <param name="extended">Indique si chaque descripteur fournit une taille stockée.</param>
    /// <param name="trackIndex">Index linéaire de la piste.</param>
    /// <param name="position">Position du bloc d'informations de piste.</param>
    /// <param name="trackSize">Taille totale du bloc de piste.</param>
    /// <param name="cylinder">Cylindre déclaré dans l'en-tête de piste.</param>
    /// <param name="head">Face déclarée dans l'en-tête de piste.</param>
    /// <param name="sectorCount">Nombre de descripteurs déclarés.</param>
    /// <param name="blocks">Blocs sectoriels auxquels ajouter les secteurs lus.</param>
    /// <param name="sectorSizes">Occurrences à mettre à jour pour chaque taille nominale.</param>
    private static void ReadSectorDescriptors(byte[] bytes, bool extended, int trackIndex, int position, int trackSize, int cylinder, int head, int sectorCount, List<SectorBlock> blocks, Dictionary<int, int> sectorSizes)
    {
        if (position + CpcDskLayout.SectorDescriptorTableOffset + sectorCount * CpcDskLayout.SectorDescriptorSize > position + CpcDskLayout.TrackInformationBlockSize) throw CpcDskExceptions.InvalidSectorTable(trackIndex);
        var dataPosition = position + CpcDskLayout.TrackInformationBlockSize;
        var trackSectors = new List<(int Cylinder, int Head, int Id, byte[] Data, bool Valid)>();
        for (var sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
        {
            var descriptor = position + CpcDskLayout.SectorDescriptorTableOffset + sectorIndex * CpcDskLayout.SectorDescriptorSize;
            var sectorCylinder = bytes[descriptor + CpcDskLayout.SectorCylinderOffset];
            var sectorHead = bytes[descriptor + CpcDskLayout.SectorHeadOffset];
            var sectorId = bytes[descriptor + CpcDskLayout.SectorIdOffset];
            var sizeCode = bytes[descriptor + CpcDskLayout.SectorSizeCodeOffset] & CpcDskLayout.SectorSizeCodeMask;
            var nominalSize = CpcDskLayout.MinimumSectorSize << sizeCode;
            var storedSize = extended ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(descriptor + CpcDskLayout.SectorStoredSizeOffset, CpcDskLayout.StoredSizeFieldLength)) : nominalSize;
            if (storedSize == 0) storedSize = nominalSize;
            if (dataPosition + storedSize > position + trackSize) throw CpcDskExceptions.TruncatedSector(cylinder, head, sectorId);
            var data = bytes.AsSpan(dataPosition, Math.Min(nominalSize, storedSize)).ToArray();
            var status1 = bytes[descriptor + CpcDskLayout.SectorStatus1Offset];
            var status2 = bytes[descriptor + CpcDskLayout.SectorStatus2Offset];
            var integrityValid = (status1 & CpcDskLayout.DataErrorMask) == 0 && (status2 & CpcDskLayout.DataErrorMask) == 0;
            trackSectors.Add((sectorCylinder, sectorHead, sectorId, data, integrityValid));
            sectorSizes[nominalSize] = sectorSizes.GetValueOrDefault(nominalSize) + 1;
            dataPosition += storedSize;
        }
        foreach (var sector in trackSectors.OrderBy(sector => sector.Id)) blocks.Add(new(blocks.Count, new(sector.Cylinder, sector.Head, sector.Id), sector.Data, sector.Valid));
    }
}
