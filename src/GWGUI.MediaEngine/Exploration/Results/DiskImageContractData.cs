using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Construit le contrat public complet depuis les modèles internes déjà validés.</summary>
internal static class DiskImageContractData
{
    private static readonly IReadOnlyDictionary<string, string> EmptyParameters = new Dictionary<string, string>();

    public static IMetadonneesImage CreateMetadata(string sourcePath, SectorImage image, ScpImage? scp)
    {
        if (scp is not null)
        {
            var header = scp.Header;
            var properties = new Dictionary<string, string>
            {
                ["flags"] = ((byte)header.Flags).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["bitCellEncoding"] = ((byte)header.BitCellEncoding).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["heads"] = ((byte)header.Heads).ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            return new MetadataData(
                "SCP",
                header.DiskType.ToString(System.Globalization.CultureInfo.InvariantCulture),
                header.ResolutionNanoseconds,
                header.Revolutions,
                header.StartTrack,
                header.EndTrack,
                scp.Tracks.Count,
                scp.Tracks.Select(track => track.Head).Distinct().Count(),
                true,
                header.Checksum.ToString("X8", System.Globalization.CultureInfo.InvariantCulture),
                null,
                scp.ChecksumValid,
                properties);
        }

        return new MetadataData(
            null,
            null,
            null,
            null,
            null,
            null,
            image.Cylinders * image.Heads,
            image.Heads,
            false,
            null,
            null,
            null,
            new Dictionary<string, string>
            {
                ["extension"] = Path.GetExtension(sourcePath)
            });
    }

    public static IReadOnlyList<IPiste> CreateTracks(SectorImage image, ScpImage? scp)
    {
        if (scp is not null)
        {
            return scp.Tracks.Select(track => FromScpTrack(track, scp.Header.ResolutionNanoseconds)).ToArray();
        }

        return Enumerable.Range(0, image.Cylinders)
            .SelectMany(cylinder => Enumerable.Range(0, image.Heads).Select(head => FromSectorTrack(image, cylinder, head)))
            .ToArray();
    }

    public static IPiste FromScpTrack(ScpTrack track, int resolutionNanoseconds)
    {
        return DiskTrackContractMapper.FromScpTrack(track, resolutionNanoseconds);
    }

    public static IReadOnlyList<IFormatDetecte> CreateFormats(
        SectorImage primaryImage,
        FileSystemVolume primaryVolume,
        DiskImageMetadata metadata,
        bool fileSystemRecognized,
        IReadOnlyList<ExploredFileSystem> fileSystems,
        IReadOnlyList<SectorImage> decodedImages)
    {
        var resolver = new DiskSystemResolver();
        var results = new List<IFormatDetecte>();
        var represented = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var fileSystem in fileSystems)
        {
            results.Add(CreateFormat(
                fileSystem.Image.WithFormatId(fileSystem.FormatId),
                fileSystem.Volume,
                metadata,
                resolver));
            represented.Add(fileSystem.FormatId);
        }

        foreach (var decodedImage in decodedImages)
        {
            if (!represented.Add(decodedImage.FormatId))
            {
                continue;
            }

            results.Add(CreateFormat(decodedImage, null, metadata, resolver));
        }

        if (results.Count == 0)
        {
            results.Add(CreateFormat(primaryImage, fileSystemRecognized ? primaryVolume : null, metadata, resolver));
        }

        return results;
    }

    public static IReadOnlyList<IDiagnostic> CreateDiagnostics(IEnumerable<string> warnings)
    {
        return warnings.Select(warning =>
            (IDiagnostic)new DiagnosticData("warning", warning, EmptyParameters, null, null, null, null)).ToArray();
    }

    private static IFormatDetecte CreateFormat(
        SectorImage image,
        FileSystemVolume? volume,
        DiskImageMetadata metadata,
        DiskSystemResolver resolver)
    {
        var sectors = Enumerable.Range(0, image.BlockCount).Select(logicalBlock => CreateSector(image, logicalBlock)).ToArray();
        var entries = volume?.Entries.Select(CreateEntry).ToArray() ?? [];
        var validCount = sectors.Count(sector => sector.Etat == "available" && sector.DonneesValides != false);
        var invalidCount = sectors.Count(sector => sector.Etat == "invalid");
        long? freeBytes = volume is { FreeSpaceKnown: true } ? volume.FreeBytes : null;
        long? usedBytes = volume is { FreeSpaceKnown: true }
            ? Math.Max(0, volume.Capacity - volume.FreeBytes)
            : null;
        return new FormatData(
            resolver.ResolveId(image.FormatId) ?? string.Empty,
            image.FormatId,
            EncodingId(image.FormatId),
            image.Cylinders,
            image.Heads,
            image.SectorsPerTrack,
            image.BlockSize,
            image.Capacity,
            validCount,
            invalidCount,
            image.MissingBlocks.Count,
            sectors,
            volume?.FileSystemId,
            string.IsNullOrWhiteSpace(volume?.Name) ? null : volume.Name,
            volume?.Capacity,
            usedBytes,
            freeBytes,
            volume?.Created,
            volume?.Modified,
            volume?.Attributes ?? [],
            volume?.Bootable ?? (metadata.Content.HasValidAmigaBootLoader ? true : null),
            volume?.DiskNumber,
            volume?.DiskCount,
            volume?.DiskNumberOrigin,
            CountEntries(volume?.Entries ?? []),
            metadata.Content.OrganizationId,
            null,
            metadata.Content.CompressionIds.ToArray(),
            metadata.Content.ModificationId,
            metadata.ProtectionId,
            entries,
            CreateDiagnostics(volume?.Warnings ?? []));
    }

    private static ISecteur CreateSector(SectorImage image, int logicalBlock)
    {
        if (!image.TryGetBlock(logicalBlock, out var block))
        {
            var address = AddressFor(image, logicalBlock);
            return new SectorData(logicalBlock, address.Cylinder, address.Head, address.Number, image.BlockSize, "missing", ReadOnlyMemory<byte>.Empty, null, null, null, null, null, []);
        }

        var state = block.IntegrityValid == false ? "invalid" : "available";
        var tag = block.Tag is null ? null : new ReadOnlyMemory<byte>(block.Tag.ToArray());
        return new SectorData(
            block.LogicalBlock,
            block.Address.Cylinder,
            block.Address.Head,
            block.Address.Number,
            block.Data.Count,
            state,
            block.Data.ToArray(),
            null,
            block.IntegrityValid,
            tag,
            block.FormatCode,
            block.DiagnosticCode,
            [block.Revolution]);
    }

    private static IEntree CreateEntry(FileSystemEntry entry)
    {
        var data = entry.Content is null ? null : new ReadOnlyMemory<byte>(entry.Content.ToArray());
        return new EntryData(
            entry.Name,
            entry.Kind.ToString(),
            entry.NativeTypeId,
            entry.Size,
            entry.OccupiedSize,
            entry.Created,
            entry.Modified,
            entry.Accessed,
            string.IsNullOrWhiteSpace(entry.Comment) ? null : entry.Comment,
            entry.Attributes,
            entry.RawAttributes,
            entry.StorageReference,
            entry.MetadataValid,
            entry.DataValid ?? (entry.Kind == FileSystemEntryKind.Directory ? null : entry.Content is not null),
            entry.SyntheticName,
            entry.LinkTarget,
            data,
            entry.Children.Select(CreateEntry).ToArray(),
            CreateDiagnostics(entry.Diagnostics));
    }

    private static IPiste FromSectorTrack(SectorImage image, int cylinder, int head)
    {
        var sectors = image.AvailableBlocks
            .Where(block => block.Address.Cylinder == cylinder && block.Address.Head == head)
            .OrderBy(block => block.Address.Number)
            .Select(block => (ISecteurSource)new SourceSectorData(block.Address.Number, block.Data.Count, block.Data.ToArray()))
            .ToArray();
        return new TrackData(null, cylinder, head, [], sectors);
    }

    private static SectorAddress AddressFor(SectorImage image, int logicalBlock)
    {
        var track = logicalBlock / image.SectorsPerTrack;
        var cylinder = track / image.Heads;
        var head = track % image.Heads;
        var sector = logicalBlock % image.SectorsPerTrack;
        return new SectorAddress(cylinder, head, sector);
    }

    private static int CountEntries(IEnumerable<FileSystemEntry> entries)
    {
        return entries.Sum(entry => 1 + CountEntries(entry.Children));
    }

    private static string EncodingId(string formatId)
    {
        if (formatId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) return "amiga-mfm";
        if (formatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) || formatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)) return "ibm-mfm";
        return string.Empty;
    }

    private sealed record MetadataData(string? Signature, string? TypeDisquette, int? ResolutionNanosecondes, int? NombreRevolutions, int? PremierePiste, int? DernierePiste, int NombrePistes, int? NombreFaces, bool ChecksumPresent, string? ChecksumDeclare, string? ChecksumCalcule, bool? ChecksumValide, IReadOnlyDictionary<string, string> ProprietesFormat) : IMetadonneesImage;
    private sealed record TrackData(int? NumeroSource, int Cylindre, int Face, IReadOnlyList<IRevolution> Revolutions, IReadOnlyList<ISecteurSource> SecteursSource) : IPiste;
    private sealed record SourceSectorData(int Numero, int Taille, ReadOnlyMemory<byte> Donnees) : ISecteurSource;
    private sealed record FormatData(string MachineId, string FormatId, string Encodage, int Cylindres, int Faces, int? SecteursParPiste, int? TailleSecteur, long CapaciteOctets, int NombreSecteursValides, int NombreSecteursInvalides, int NombreSecteursAbsents, IReadOnlyList<ISecteur> Secteurs, string? SystemeFichiers, string? NomVolume, long? CapaciteVolume, long? EspaceUtilise, long? EspaceLibre, DateTimeOffset? CreationVolume, DateTimeOffset? ModificationVolume, IReadOnlyList<string> AttributsVolume, bool? Amorcable, int? NumeroDisque, int? NombreDisques, string? OrigineNumeroDisque, int NombreEntrees, string? Organisation, string? Chargeur, IReadOnlyList<string> Compactages, string? Crack, string? Protection, IReadOnlyList<IEntree> Entrees, IReadOnlyList<IDiagnostic> Diagnostics) : IFormatDetecte;
    private sealed record SectorData(int BlocLogique, int Cylindre, int Face, int Numero, int Taille, string Etat, ReadOnlyMemory<byte> Donnees, bool? EnteteValide, bool? DonneesValides, ReadOnlyMemory<byte>? Tag, byte? CodeFormat, byte? CodeDiagnostic, IReadOnlyList<int> Revolutions) : ISecteur;
    private sealed record EntryData(string Nom, string Type, string? TypeNatifId, long Taille, long? TailleOccupee, DateTimeOffset? Creation, DateTimeOffset? Modification, DateTimeOffset? Acces, string? Commentaire, IReadOnlyList<string> Attributs, uint? AttributsBruts, long? ReferenceStockage, bool MetadonneesValides, bool? DonneesValides, bool NomSynthetique, string? CibleLien, ReadOnlyMemory<byte>? Donnees, IReadOnlyList<IEntree> Enfants, IReadOnlyList<IDiagnostic> Diagnostics) : IEntree;
    private sealed record DiagnosticData(string Niveau, string Code, IReadOnlyDictionary<string, string> Parametres, int? Cylindre, int? Face, int? Revolution, int? Secteur) : IDiagnostic;
}
