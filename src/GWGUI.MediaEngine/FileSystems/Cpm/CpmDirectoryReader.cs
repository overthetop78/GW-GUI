using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Fournit les opérations communes de lecture des répertoires et extents CP/M.</summary>
internal static class CpmDirectoryReader
{
    /// <summary>Image logique positionnelle et état de présence de ses blocs.</summary>
    internal sealed record LogicalImage(byte[] Bytes, int BlockSize, IReadOnlySet<int> MissingBlocks, IReadOnlySet<int> TruncatedBlocks)
    {
        /// <summary>Indique si toute une plage provient de blocs complets.</summary>
        public bool IsAvailable(int offset, int length)
        {
            if (offset < 0 || length < 0 || offset > Bytes.Length - length) return false;
            if (length == 0) return true;
            var first = offset / BlockSize;
            var last = (offset + length - 1) / BlockSize;
            return Enumerable.Range(first, last - first + 1).All(block => !MissingBlocks.Contains(block) && !TruncatedBlocks.Contains(block));
        }
    }

    /// <summary>Répertoire CP/M décodé.</summary>
    internal sealed record DirectoryResult(string VolumeName, IReadOnlyList<CpmExtent> Extents);

    /// <summary>Résultat positionnel de la reconstruction d'un fichier.</summary>
    internal sealed record FileResult(IReadOnlyList<byte> Content, bool Valid, IReadOnlySet<int> UsedAllocations, bool Rejected);

    /// <summary>Aplatit une image sans déplacer les blocs suivant un bloc absent ou tronqué.</summary>
    public static LogicalImage Flatten(SectorImage image)
    {
        var bytes = new byte[checked(image.BlockCount * image.BlockSize)];
        var missing = new HashSet<int>();
        var truncated = new HashSet<int>();
        for (var logicalBlock = 0; logicalBlock < image.BlockCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block)) { missing.Add(logicalBlock); continue; }
            var count = Math.Min(block.Data.Count, image.BlockSize);
            block.Data.Take(count).ToArray().CopyTo(bytes, logicalBlock * image.BlockSize);
            if (block.Data.Count != image.BlockSize) truncated.Add(logicalBlock);
        }
        return new(bytes, image.BlockSize, missing, truncated);
    }

    /// <summary>Calcule le nombre d'entrées ordinaires plausibles d'un répertoire.</summary>
    public static int ScoreDirectory(LogicalImage image, CpmLayout layout, bool rejectLowercase)
    {
        var score = 0;
        for (var index = 0; index < layout.DirectoryEntries; index++)
        {
            var offset = layout.DirectoryOffset + index * CpmFormat.DirectoryEntrySize;
            if (!image.IsAvailable(offset, CpmFormat.DirectoryEntrySize)) continue;
            var entry = image.Bytes.AsSpan(offset, CpmFormat.DirectoryEntrySize);
            if (entry[0] <= CpmFormat.MaximumUserNumber && TryDecodeName(entry, rejectLowercase, out _)) score++;
        }
        return score;
    }

    /// <summary>Vérifie la plausibilité complète d'une fenêtre de répertoire.</summary>
    public static bool LooksLikeDirectory(LogicalImage image, CpmLayout layout, bool allowEmpty, bool rejectLowercase)
    {
        var active = 0;
        var unused = 0;
        for (var index = 0; index < layout.DirectoryEntries; index++)
        {
            var offset = layout.DirectoryOffset + index * CpmFormat.DirectoryEntrySize;
            if (!image.IsAvailable(offset, CpmFormat.DirectoryEntrySize)) return false;
            var entry = image.Bytes.AsSpan(offset, CpmFormat.DirectoryEntrySize);
            if (entry[0] == CpmFormat.UnusedEntryMarker) { unused++; continue; }
            if (entry[0] <= CpmFormat.MaximumUserNumber && TryDecodeName(entry, rejectLowercase, out _)) active++;
            else if (entry[0] is not (CpmFormat.VolumeLabelUser or CpmFormat.PasswordLabelUser)) return false;
        }
        return active > 0 || allowEmpty && unused == layout.DirectoryEntries;
    }

    /// <summary>Recherche un répertoire sur les frontières indiquées.</summary>
    public static CpmLayout? FindDirectory(LogicalImage image, CpmLayout template, int step, bool allowEmpty, bool rejectLowercase)
    {
        var length = template.DirectoryEntries * CpmFormat.DirectoryEntrySize;
        for (var offset = 0; offset <= image.Bytes.Length - length; offset += step)
        {
            var candidate = template with { DirectoryOffset = offset, AllocationOrigin = offset };
            if (LooksLikeDirectory(image, candidate, allowEmpty, rejectLowercase)) return candidate;
        }
        return null;
    }

    /// <summary>Lit le label et les extents d'un répertoire.</summary>
    public static DirectoryResult ReadDirectory(LogicalImage image, CpmLayout layout, bool rejectLowercase)
    {
        var extents = new List<CpmExtent>();
        var volumeName = string.Empty;
        for (var index = 0; index < layout.DirectoryEntries; index++)
        {
            var offset = layout.DirectoryOffset + index * CpmFormat.DirectoryEntrySize;
            if (!image.IsAvailable(offset, CpmFormat.DirectoryEntrySize)) continue;
            var entry = image.Bytes.AsSpan(offset, CpmFormat.DirectoryEntrySize);
            var user = entry[0];
            if (user == CpmFormat.UnusedEntryMarker || user == CpmFormat.PasswordLabelUser) continue;
            if (user == CpmFormat.VolumeLabelUser)
            {
                var candidate = DecodePart(entry.Slice(CpmFormat.FileNameOffset, CpmFormat.FileNameLength));
                if (IsPlausibleLabel(candidate)) volumeName = candidate;
                continue;
            }
            if (user > CpmFormat.MaximumUserNumber || !TryDecodeName(entry, rejectLowercase, out var name)) continue;
            var number = entry[CpmFormat.ExtentLowOffset] + (entry[CpmFormat.ExtentHighOffset] << CpmFormat.ExtentHighShift);
            extents.Add(new(user, name, number, entry[CpmFormat.RecordCountOffset], ReadAllocations(entry, layout.WideAllocations)));
        }
        return new(volumeName, extents);
    }

    /// <summary>Regroupe les extents par zone utilisateur et nom sans casse.</summary>
    public static IReadOnlyList<IGrouping<(byte User, string Name), CpmExtent>> GroupExtents(IEnumerable<CpmExtent> extents) => extents.GroupBy(extent => (extent.User, extent.Name), new CpmExtentKeyComparer()).ToArray();

    /// <summary>Reconstruit un groupe d'extents sans déplacer les allocations invalides.</summary>
    public static FileResult Reconstruct(LogicalImage image, CpmLayout layout, IGrouping<(byte User, string Name), CpmExtent> group, List<string> warnings)
    {
        var totalAllocations = Math.Max(0, (image.Bytes.Length - layout.AllocationOrigin) / layout.AllocationBlockSize);
        var referenced = group.SelectMany(extent => extent.Allocations).Where(allocation => allocation != 0).ToArray();
        var validCount = referenced.Count(allocation => IsValidAllocation(image, layout, allocation, totalAllocations));
        if (!HasPlausibleAllocationMajority(referenced.Length, validCount)) return new([], false, new HashSet<int>(), true);
        using var content = new MemoryStream();
        var valid = true;
        var usedAllocations = new HashSet<int>();
        foreach (var extent in group.OrderBy(extent => extent.Number))
        {
            using var extentBytes = new MemoryStream();
            foreach (var allocation in extent.Allocations)
            {
                if (allocation == 0) continue;
                var offset = checked(layout.AllocationOrigin + allocation * layout.AllocationBlockSize);
                if (!IsWithinAllocationRange(layout, allocation, totalAllocations))
                {
                    warnings.Add(CpmFileSystemExceptions.AllocationOutsideImage(group.Key.Name, allocation, offset, image.Bytes.Length));
                    valid = false;
                    extentBytes.Write(new byte[layout.AllocationBlockSize]);
                    continue;
                }
                if (!image.IsAvailable(offset, layout.AllocationBlockSize))
                {
                    var firstBlock = offset / image.BlockSize;
                    var lastBlock = (offset + layout.AllocationBlockSize - 1) / image.BlockSize;
                    var crossesMissing = Enumerable.Range(firstBlock, lastBlock - firstBlock + 1).Any(image.MissingBlocks.Contains);
                    warnings.Add(crossesMissing ? CpmFileSystemExceptions.MissingLogicalBlock(group.Key.Name, allocation) : CpmFileSystemExceptions.TruncatedLogicalBlock(group.Key.Name, allocation));
                    valid = false;
                    extentBytes.Write(new byte[layout.AllocationBlockSize]);
                    continue;
                }
                if (!usedAllocations.Add(allocation)) warnings.Add(CpmFileSystemExceptions.DuplicateAllocation(group.Key.Name, allocation));
                extentBytes.Write(image.Bytes, offset, layout.AllocationBlockSize);
            }
            var used = Math.Min(extentBytes.Length, extent.RecordCount * (long)CpmFormat.RecordSize);
            content.Write(extentBytes.GetBuffer(), 0, checked((int)used));
        }
        return new(content.ToArray(), valid, usedAllocations, false);
    }

    /// <summary>Décode un nom CP/M 8.3 et applique la règle de casse demandée.</summary>
    public static bool TryDecodeName(ReadOnlySpan<byte> entry, bool rejectLowercase, out string name)
    {
        name = string.Empty;
        if (entry.Length < CpmFormat.DirectoryEntrySize) return false;
        for (var index = CpmFormat.FileNameOffset; index < CpmFormat.AllocationOffset - 4; index++)
        {
            var value = entry[index] & CpmFormat.AttributeBitMask;
            if (value != 0x20 && (value < 0x21 || value > 0x7e)) return false;
            if (rejectLowercase && value is >= (byte)'a' and <= (byte)'z') return false;
        }
        var stem = DecodePart(entry.Slice(CpmFormat.FileNameOffset, CpmFormat.FileNameLength));
        var extension = DecodePart(entry.Slice(CpmFormat.FileExtensionOffset, CpmFormat.FileExtensionLength));
        if (stem.Length == 0) return false;
        name = extension.Length == 0 ? stem : stem + "." + extension;
        return true;
    }

    /// <summary>Décode une partie de nom en retirant les bits d'attribut.</summary>
    public static string DecodePart(ReadOnlySpan<byte> value)
    {
        Span<byte> clean = stackalloc byte[value.Length];
        for (var index = 0; index < value.Length; index++) clean[index] = (byte)(value[index] & CpmFormat.AttributeBitMask);
        return System.Text.Encoding.ASCII.GetString(clean).Trim();
    }

    /// <summary>Lit les allocations étroites ou larges d'une entrée.</summary>
    public static IReadOnlyList<int> ReadAllocations(ReadOnlySpan<byte> entry, bool wide)
    {
        var count = wide ? CpmFormat.WideAllocationCount : CpmFormat.NarrowAllocationCount;
        var allocations = new int[count];
        for (var index = 0; index < count; index++) allocations[index] = wide ? BinaryPrimitives.ReadUInt16LittleEndian(entry.Slice(CpmFormat.AllocationOffset + index * CpmFormat.WideAllocationSize)) : entry[CpmFormat.AllocationOffset + index];
        return allocations;
    }

    /// <summary>Vérifie qu'un label contient uniquement les caractères acceptés.</summary>
    public static bool IsPlausibleLabel(string value) => value.Length > 0 && value.All(character => char.IsLetterOrDigit(character) || character is ' ' or '-' or '_' or '.');

    private static bool IsValidAllocation(LogicalImage image, CpmLayout layout, int allocation, int totalAllocations)
    {
        if (!IsWithinAllocationRange(layout, allocation, totalAllocations)) return false;
        var offset = checked(layout.AllocationOrigin + allocation * layout.AllocationBlockSize);
        return image.IsAvailable(offset, layout.AllocationBlockSize);
    }

    private static bool IsWithinAllocationRange(CpmLayout layout, int allocation, int totalAllocations) => allocation > 0 && allocation < totalAllocations;

    /// <summary>Évite les faux positifs de répertoire en rejetant un fichier dont la majorité des allocations référencées est invalide.</summary>
    private static bool HasPlausibleAllocationMajority(int referencedCount, int validCount) => referencedCount == 0 || validCount * 2 >= referencedCount;
}
