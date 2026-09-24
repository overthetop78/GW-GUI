namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Analyse les propriétaires et les recouvrements après lecture de toutes les chaînes du répertoire.</summary>
public static class AtariDosAllocationAnalyzer
{
    /// <summary>Distingue les vues sectorielles partagées des propriétaires réellement incohérents.</summary>
    public static IReadOnlyDictionary<int, AtariDosFileData> Analyze(
        IReadOnlyList<AtariDosDirectoryEntry> entries,
        IReadOnlyDictionary<int, AtariDosFileData> files,
        ICollection<string> warnings)
    {
        var entriesByNumber = entries.ToDictionary(entry => entry.EntryNumber);
        var result = new Dictionary<int, AtariDosFileData>();
        foreach (var entry in entries)
        {
            var file = files[entry.EntryNumber];
            var attributes = file.Attributes.ToList();
            var diagnostics = file.Diagnostics.ToList();
            var valid = file.IsValid;

            if (file.Chain.ContinuationSector != 0)
            {
                var diagnostic = AtariDosWarnings.ContinuationAfterDeclaredExtent(
                    entry.Name,
                    entry.DeclaredSectorCount,
                    file.Chain.ContinuationSector);
                warnings.Add(diagnostic);
                diagnostics.Add(diagnostic);
                attributes.Add(AtariDosFileSystemLayout.BoundedSectorViewAttribute);
            }

            var mismatches = Enumerable.Range(0, file.Chain.SectorNumbers.Count)
                .Where(index => file.Chain.StoredFileNumbers[index].HasValue
                    && file.Chain.StoredFileNumbers[index] != entry.EntryNumber)
                .ToArray();
            if (mismatches.Length != 0)
            {
                var observedOwners = mismatches
                    .Select(index => file.Chain.StoredFileNumbers[index]!.Value)
                    .Distinct()
                    .ToArray();
                if (observedOwners.Length == 1
                    && entriesByNumber.TryGetValue(observedOwners[0], out var ownerEntry)
                    && files.TryGetValue(observedOwners[0], out var ownerFile)
                    && ContainsContiguous(ownerFile.Chain.SectorNumbers, file.Chain.SectorNumbers))
                {
                    var diagnostic = AtariDosWarnings.SharedSectorView(
                        entry.Name,
                        ownerEntry.Name,
                        file.Chain.SectorNumbers[0],
                        file.Chain.SectorNumbers.Count);
                    warnings.Add(diagnostic);
                    diagnostics.Add(diagnostic);
                    attributes.Add(AtariDosFileSystemLayout.SharedSectorViewAttribute);
                }
                else
                {
                    foreach (var index in mismatches)
                    {
                        var diagnostic = AtariDosFileSystemExceptions.InconsistentOwner(
                            entry.Name,
                            file.Chain.SectorNumbers[index],
                            entry.EntryNumber,
                            file.Chain.StoredFileNumbers[index]!.Value);
                        warnings.Add(diagnostic);
                        diagnostics.Add(diagnostic);
                    }
                    valid = false;
                }
            }

            result.Add(entry.EntryNumber, file with
            {
                IsValid = valid,
                Attributes = Array.AsReadOnly(attributes.Distinct(StringComparer.Ordinal).ToArray()),
                Diagnostics = Array.AsReadOnly(diagnostics.ToArray())
            });
        }
        return result;
    }

    private static bool ContainsContiguous(IReadOnlyList<int> containing, IReadOnlyList<int> candidate)
    {
        if (candidate.Count == 0 || candidate.Count > containing.Count) return false;
        for (var start = 0; start <= containing.Count - candidate.Count; start++)
        {
            var matches = true;
            for (var index = 0; index < candidate.Count; index++)
            {
                if (containing[start + index] == candidate[index]) continue;
                matches = false;
                break;
            }
            if (matches) return true;
        }
        return false;
    }
}
