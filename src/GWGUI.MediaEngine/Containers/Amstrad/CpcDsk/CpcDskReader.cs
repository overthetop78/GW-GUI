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
        if (bytes.Length < CpcDskLayout.DiskInformationBlockSize) throw CpcDskExceptions.TruncatedHeader();
        var signature = bytes.AsSpan(0, CpcDskLayout.DiskSignatureLength);
        var extended = signature.StartsWith(CpcDskFormat.ExtendedSignatureBytes);
        if (!extended && !signature.StartsWith(CpcDskFormat.StandardSignatureBytes)) throw CpcDskExceptions.UnrecognizedSignature();

        var cylinders = bytes[CpcDskLayout.CylinderCountOffset];
        var heads = bytes[CpcDskLayout.HeadCountOffset];
        if (cylinders is < CpcDskLayout.MinimumCylinderCount or > CpcDskLayout.MaximumCylinderCount || heads is < CpcDskLayout.MinimumHeadCount or > CpcDskLayout.MaximumHeadCount) throw CpcDskExceptions.InvalidGeometry();
        var trackCount = checked(cylinders * heads);
        if (extended && CpcDskLayout.ExtendedTrackSizeTableOffset + trackCount > CpcDskLayout.DiskInformationBlockSize)
            throw CpcDskExceptions.InvalidExtendedTrackTable();

        var blocks = new List<SectorBlock>();
        var position = CpcDskLayout.DiskInformationBlockSize;
        var logicalBlock = 0;
        var maximumSectors = 0;
        var sectorSizes = new Dictionary<int, int>();
        for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var trackSize = extended
                ? bytes[CpcDskLayout.ExtendedTrackSizeTableOffset + trackIndex] * CpcDskLayout.ExtendedTrackSizeUnit
                : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(CpcDskLayout.StandardTrackSizeOffset, CpcDskLayout.StoredSizeFieldLength));
            if (trackSize == 0) continue;
            if (position + trackSize > bytes.Length || trackSize < CpcDskLayout.TrackInformationBlockSize)
                throw CpcDskExceptions.TruncatedTrack(trackIndex);
            if (!bytes.AsSpan(position, CpcDskLayout.TrackSignatureLength).StartsWith(CpcDskFormat.TrackSignatureBytes)) throw CpcDskExceptions.InvalidTrackHeader(trackIndex);

            var cylinder = bytes[position + CpcDskLayout.TrackCylinderOffset];
            var head = bytes[position + CpcDskLayout.TrackHeadOffset];
            var sectorCount = bytes[position + CpcDskLayout.TrackSectorCountOffset];
            maximumSectors = Math.Max(maximumSectors, sectorCount);
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
                var storedSize = extended
                    ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(descriptor + CpcDskLayout.SectorStoredSizeOffset, CpcDskLayout.StoredSizeFieldLength))
                    : nominalSize;
                if (storedSize == 0) storedSize = nominalSize;
                if (dataPosition + storedSize > position + trackSize)
                    throw CpcDskExceptions.TruncatedSector(cylinder, head, sectorId);
                var data = bytes.AsSpan(dataPosition, Math.Min(nominalSize, storedSize)).ToArray();
                var status1 = bytes[descriptor + CpcDskLayout.SectorStatus1Offset];
                var status2 = bytes[descriptor + CpcDskLayout.SectorStatus2Offset];
                var integrityValid = (status1 & CpcDskLayout.DataErrorMask) == 0 && (status2 & CpcDskLayout.DataErrorMask) == 0;
                trackSectors.Add((sectorCylinder, sectorHead, sectorId, data, integrityValid));
                sectorSizes[nominalSize] = sectorSizes.GetValueOrDefault(nominalSize) + 1;
                dataPosition += storedSize;
            }
            foreach (var sector in trackSectors.OrderBy(sector => sector.Id))
                blocks.Add(new(logicalBlock++, new(sector.Cylinder, sector.Head, sector.Id), sector.Data, sector.Valid));
            position += trackSize;
        }
        if (blocks.Count == 0) throw CpcDskExceptions.NoSectors();
        var dominantSize = sectorSizes.OrderByDescending(item => item.Value).First().Key;
        return new(CpcDskFormat.FormatId, dominantSize, cylinders, heads, Math.Max(1, maximumSectors), blocks,
            allowVariableBlockSize: sectorSizes.Count > 1, capacity: blocks.Sum(block => (long)block.Data.Count), logicalBlockCount: blocks.Count);
    }
}
