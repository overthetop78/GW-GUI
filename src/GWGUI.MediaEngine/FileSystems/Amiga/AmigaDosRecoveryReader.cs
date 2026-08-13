using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Récupère les entrées AmigaDOS dont les en-têtes restent valides lorsque la racine est indisponible.</summary>
public static class AmigaDosRecoveryReader
{
    /// <summary>Tente de reconstruire un catalogue partiel uniquement à partir d'en-têtes cohérents et contrôlés.</summary>
    public static bool TryRead(SectorImage image, out FileSystemVolume? volume)
    {
        volume = null;
        if (!TryReadBoot(image, out var variant, out var expectedRoot) || !IsRootUnavailable(image, expectedRoot)) return false;
        var candidates = ReadCandidates(image, variant);
        if (!candidates.Values.Any(candidate => candidate.ParentBlock == expectedRoot)) return false;
        var warnings = new List<string>();
        var roots = candidates.Values.Where(candidate => candidate.ParentBlock == expectedRoot || !candidates.ContainsKey(candidate.ParentBlock) || candidates[candidate.ParentBlock].Kind != FileSystemEntryKind.Directory).ToArray();
        var entries = roots.Select(candidate => BuildEntry(image, candidate, candidates, variant, warnings, new HashSet<int>())).OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        warnings.Insert(0, AmigaDosWarnings.CatalogRecoveredWithoutRoot(candidates.Count));
        volume = new FileSystemVolume(string.Empty, variant.FileSystemId(), image.Capacity, 0, null, null, entries, warnings, freeSpaceKnown: false);
        return true;
    }

    private static bool TryReadBoot(SectorImage image, out AmigaDosVariant variant, out int expectedRoot)
    {
        variant = AmigaDosVariant.Ofs;
        expectedRoot = 0;
        if (image.BlockSize != AmigaDosLayout.BlockSize || !image.TryGetBlock(AmigaDosLayout.BootBlock, out var bootBlock) || bootBlock.Data.Count != AmigaDosLayout.BlockSize) return false;
        var boot = bootBlock.Data.ToArray();
        if (!AmigaDosRootBlockReader.HasDosPrefix(boot) || boot[AmigaDosLayout.DosVariantOffset] > (byte)AmigaDosLayout.MaximumVariant) return false;
        variant = (AmigaDosVariant)boot[AmigaDosLayout.DosVariantOffset];
        var declaredRoot = BigEndianInt32.Read(boot, AmigaDosLayout.BootRootPointerOffset);
        expectedRoot = declaredRoot > 0 && declaredRoot < image.BlockCount ? declaredRoot : image.BlockCount / 2;
        return expectedRoot > 0 && expectedRoot < image.BlockCount;
    }

    private static bool IsRootUnavailable(SectorImage image, int expectedRoot) => !image.TryGetBlock(expectedRoot, out var root) || root.IntegrityValid == false;

    private static Dictionary<int, RecoveredEntry> ReadCandidates(SectorImage image, AmigaDosVariant variant)
    {
        var candidates = new Dictionary<int, RecoveredEntry>();
        foreach (var sector in image.AvailableBlocks)
        {
            if (sector.IntegrityValid != true || sector.Data.Count != AmigaDosLayout.BlockSize) continue;
            var block = sector.Data.ToArray();
            if (BigEndianInt32.Read(block, AmigaDosLayout.PrimaryTypeOffset) != AmigaDosLayout.HeaderPrimaryType || BigEndianInt32.Read(block, AmigaDosLayout.HeaderKeyOffset) != sector.LogicalBlock || !AmigaDosChecksum.IsValid(block)) continue;
            var kind = AmigaDosEntryTypeExtensions.FromRaw(BigEndianInt32.Read(block, AmigaDosLayout.SecondaryTypeOffset)).ToCommonKind();
            if (kind is not (FileSystemEntryKind.Directory or FileSystemEntryKind.File)) continue;
            var parent = BigEndianInt32.Read(block, AmigaDosLayout.ParentBlockOffset);
            var name = AmigaDosNameCodec.ReadEntryName(block, variant);
            if (parent <= 0 || parent >= image.BlockCount || string.IsNullOrWhiteSpace(name) || name.Any(character => char.IsControl(character))) continue;
            candidates.Add(sector.LogicalBlock, new(sector.LogicalBlock, parent, name, kind, block));
        }
        return candidates;
    }

    private static FileSystemEntry BuildEntry(SectorImage image, RecoveredEntry candidate, IReadOnlyDictionary<int, RecoveredEntry> candidates, AmigaDosVariant variant, List<string> warnings, HashSet<int> ancestors)
    {
        if (!ancestors.Add(candidate.BlockNumber)) return CreateEntry(candidate, [], null, false);
        var children = candidate.Kind == FileSystemEntryKind.Directory ? candidates.Values.Where(child => child.ParentBlock == candidate.BlockNumber).Select(child => BuildEntry(image, child, candidates, variant, warnings, new HashSet<int>(ancestors))).OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray() : [];
        if (candidate.Kind != FileSystemEntryKind.File) return CreateEntry(candidate, children, null, true);
        try
        {
            var size = checked((int)BigEndianInt32.ReadUnsigned(candidate.Data, AmigaDosLayout.FileSizeOffset));
            var file = AmigaDosFileReader.Read(image, candidate.Data, size, variant, warnings);
            return CreateEntry(candidate, children, file.Content, file.IsValid);
        }
        catch (Exception exception) when (exception is InvalidDataException or OverflowException or ArgumentOutOfRangeException)
        {
            warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(candidate.Name, exception));
            return CreateEntry(candidate, children, null, false);
        }
    }

    private static FileSystemEntry CreateEntry(RecoveredEntry candidate, IReadOnlyList<FileSystemEntry> children, IReadOnlyList<byte>? content, bool metadataValid)
    {
        var size = candidate.Kind == FileSystemEntryKind.File ? BigEndianInt32.ReadUnsigned(candidate.Data, AmigaDosLayout.FileSizeOffset) : 0;
        return new(candidate.Name, candidate.Kind, size, AmigaDosTime.Read(candidate.Data, AmigaDosLayout.DateOffset), AmigaDosNameCodec.Read(candidate.Data, AmigaDosLayout.LongNameOffset, AmigaDosLayout.CommentMaximumLength), BigEndianInt32.ReadUnsigned(candidate.Data, AmigaDosLayout.ProtectionOffset), candidate.BlockNumber, metadataValid, children, content);
    }

    private sealed record RecoveredEntry(int BlockNumber, int ParentBlock, string Name, FileSystemEntryKind Kind, byte[] Data);
}
