using System.Buffers.Binary;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Cp2;

/// <summary>Lit les secteurs capturés par SNATCH-IT pour Copy II PC.</summary>
public sealed class Cp2Reader
{
    /// <summary>Lit, valide et reconstruit une image sectorielle depuis un conteneur CP2.</summary>
    /// <param name="path">Chemin du conteneur CP2 à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>L'image sectorielle reconstruite à partir des secteurs CP2 valides.</returns>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture du fichier.</exception>
    /// <exception cref="InvalidDataException">Le conteneur CP2, ses descripteurs ou sa géométrie sont invalides.</exception>
    /// <exception cref="OverflowException">Un calcul de position ou de taille dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        ValidateContainer(data);
        var sectors = ReadSectorBlocks(data, cancellationToken);
        return BuildImage(sectors, cancellationToken);
    }

    /// <summary>Valide la longueur minimale et la signature CP2.</summary>
    /// <param name="data">Octets du conteneur à valider.</param>
    /// <exception cref="InvalidDataException">Le conteneur est trop court ou sa signature CP2 est absente.</exception>
    private static void ValidateContainer(ReadOnlySpan<byte> data)
    {
        if (data.Length < Cp2Layout.MinimumFileLength || !data.Slice(Cp2Layout.SignatureOffset, Cp2Format.SignatureLength).SequenceEqual(Cp2Format.Signature)) throw Cp2Exceptions.MissingSignature();
    }

    /// <summary>Calcule la géométrie observée et reconstruit les secteurs CP2 de 512 octets.</summary>
    /// <param name="sectors">Secteurs CP2 indexés par leur adresse logique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la reconstruction IBM.</param>
    /// <returns>L'image sectorielle reconstruite.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur n'est disponible ou la géométrie calculée est invalide.</exception>
    /// <exception cref="OverflowException">La taille de l'image linéaire dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    private static SectorImage BuildImage(IReadOnlyDictionary<SectorAddress, byte[]> sectors, CancellationToken cancellationToken)
    {
        if (sectors.Count == 0) throw Cp2Exceptions.NoSectors();

        var cylinders = sectors.Keys.Max(address => address.Cylinder) + 1;
        var heads = sectors.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = sectors.Keys.Max(address => address.Number);
        if (heads is <= 0 or > 2 || sectorsPerTrack <= 0)
            throw Cp2Exceptions.InvalidGeometry(heads, sectorsPerTrack);

        var linear = new byte[checked(cylinders * heads * sectorsPerTrack * Cp2Layout.ReconstructedSectorSize)];
        foreach (var (address, bytes) in sectors)
        {
            if (bytes.Length != Cp2Layout.ReconstructedSectorSize || address.Number is <= 0 || address.Number > sectorsPerTrack) continue;
            var logical = ((address.Cylinder * heads + address.Head) * sectorsPerTrack) + address.Number - 1;
            bytes.CopyTo(linear, logical * Cp2Layout.ReconstructedSectorSize);
        }
        return IbmPcImageReader.Create(linear, cancellationToken);
    }

    /// <summary>Parcourt les groupes CP2 et lit leurs charges utiles dans l'ordre de position angulaire.</summary>
    /// <param name="data">Octets complets du conteneur CP2.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le parcours.</param>
    /// <returns>Les secteurs de 512 octets indexés par leur adresse logique.</returns>
    /// <exception cref="InvalidDataException">Un groupe, un descripteur ou une charge utile est invalide ou tronqué.</exception>
    /// <exception cref="OverflowException">Un calcul de position dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    private static Dictionary<SectorAddress, byte[]> ReadSectorBlocks(byte[] data, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, byte[]>();
        var groupOffset = Cp2Layout.FirstGroupOffset;
        while (groupOffset + Cp2Layout.GroupHeaderSize <= data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(groupOffset + Cp2Layout.MetadataLengthOffset, Cp2Layout.LengthFieldSize));
            if (metadataLength == 0 || (metadataLength - Cp2Layout.MetadataLengthAdjustment) % Cp2Layout.TrackDescriptorSize != 0) throw Cp2Exceptions.InvalidDescriptionBlock(groupOffset, metadataLength, data.Length - groupOffset);

            var descriptorCount = (metadataLength - Cp2Layout.MetadataLengthAdjustment) / Cp2Layout.TrackDescriptorSize;
            var descriptors = new List<TrackDescriptor>(descriptorCount);
            for (var index = 0; index < descriptorCount; index++)
            {
                var offset = groupOffset + Cp2Layout.GroupHeaderSize + index * Cp2Layout.TrackDescriptorSize;
                if (offset + Cp2Layout.TrackDescriptorSize > data.Length) throw Cp2Exceptions.TruncatedDescriptionBlock(offset, Cp2Layout.TrackDescriptorSize, data.Length - offset);
                var descriptor = ParseTrackDescriptor(data.AsSpan(offset, Cp2Layout.TrackDescriptorSize));
                if (descriptor.Sectors.Count != 0) descriptors.Add(descriptor);
            }

            // Deux octets situés entre les métadonnées et la charge utile appartiennent à
            // l'encadrement du bloc CP2. Les secteurs suivent dans l'ordre angulaire physique.
            var payloadOffset = checked(groupOffset + Cp2Layout.GroupHeaderSize + metadataLength + Cp2Layout.FramingSize);
            foreach (var track in descriptors)
            {
                foreach (var sector in track.Sectors.OrderBy(item => item.Position))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (payloadOffset > data.Length - sector.Size) throw Cp2Exceptions.TruncatedSectorData(new(sector.Cylinder, sector.Head, sector.Number), payloadOffset, sector.Size, data.Length - payloadOffset);
                    var bytes = data.AsSpan(payloadOffset, sector.Size).ToArray();
                    payloadOffset += sector.Size;
                    if (sector.Size == Cp2Layout.ReconstructedSectorSize)
                        result.TryAdd(new SectorAddress(sector.Cylinder, sector.Head, sector.Number), bytes);
                }
            }

            if (payloadOffset >= data.Length) break;
            // Les deux premiers octets du groupe suivant ferment la charge utile précédente ;
            // la longueur de ses métadonnées les suit immédiatement.
            groupOffset = payloadOffset - Cp2Layout.FramingSize;
        }
        return result;
    }

    /// <summary>Décode un descripteur de piste CP2 et ses descripteurs sectoriels cohérents.</summary>
    /// <param name="descriptor">Descripteur binaire complet de la piste.</param>
    /// <returns>La liste des secteurs décrits par la piste.</returns>
    /// <exception cref="InvalidDataException">Le nombre de descripteurs sectoriels est invalide.</exception>
    private static TrackDescriptor ParseTrackDescriptor(ReadOnlySpan<byte> descriptor)
    {
        var count = descriptor[Cp2Layout.TrackSectorCountOffset];
        if (count == 0) return new([]);
        if (count > Cp2Layout.MaximumSectorDescriptorCount || Cp2Layout.TrackHeaderSize + count * Cp2Layout.SectorDescriptorSize > descriptor.Length) throw Cp2Exceptions.InvalidSectorDescriptorCount(count, Cp2Layout.MaximumSectorDescriptorCount);

        var sectors = new List<Cp2SectorDescriptor>(count);
        var trackCylinder = descriptor[Cp2Layout.TrackCylinderOffset];
        var trackHead = descriptor[Cp2Layout.TrackHeadOffset];
        for (var index = 0; index < count; index++)
        {
            var record = descriptor.Slice(Cp2Layout.TrackHeaderSize + index * Cp2Layout.SectorDescriptorSize, Cp2Layout.SectorDescriptorSize);
            var sizeCode = record[Cp2Layout.SectorSizeCodeOffset];
            // SNATCH-IT termine certains fichiers par des enregistrements C/H=6 synthétiques.
            // Ils ne décrivent aucune charge utile stockée et ne doivent pas étendre la géométrie.
            if (record[Cp2Layout.SectorCylinderOffset] != trackCylinder || record[Cp2Layout.SectorHeadOffset] != trackHead || sizeCode > Cp2Layout.MaximumSectorSizeCode) continue;
            sectors.Add(new(record[Cp2Layout.SectorCylinderOffset], record[Cp2Layout.SectorHeadOffset], record[Cp2Layout.SectorNumberOffset], Cp2Layout.BaseSectorSize << sizeCode, BinaryPrimitives.ReadUInt16LittleEndian(record.Slice(Cp2Layout.SectorPositionOffset, Cp2Layout.SectorPositionLength))));
        }
        return new(sectors);
    }

    /// <summary>Regroupe les secteurs valides décrits par une piste CP2.</summary>
    /// <param name="Sectors">Secteurs de la piste.</param>
    private sealed record TrackDescriptor(IReadOnlyList<Cp2SectorDescriptor> Sectors);

    /// <summary>Décrit l'adresse, la taille et la position angulaire d'un secteur CP2.</summary>
    /// <param name="Cylinder">Numéro de cylindre.</param>
    /// <param name="Head">Numéro de face.</param>
    /// <param name="Number">Numéro logique du secteur.</param>
    /// <param name="Size">Taille de la charge utile, en octets.</param>
    /// <param name="Position">Position angulaire enregistrée dans le conteneur.</param>
    private readonly record struct Cp2SectorDescriptor(int Cylinder, int Head, int Number, int Size, int Position);
}
