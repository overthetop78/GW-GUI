namespace GWGUI.Emulation.Atari.Services;

public sealed class AtariFirmwareScanner(string firmwareRoot)
{
    private readonly string _firmwareRoot = Path.GetFullPath(firmwareRoot);

    public async Task<IReadOnlyList<AtariScannedFirmware>> ScanAsync(AtariMachineModel model,
        AtariStRegion? region = null, CancellationToken cancellationToken = default)
    {
        var candidates = await Task.Run(() => AtariFirmwareScanFunctions.EnumerateCandidates(_firmwareRoot),
            cancellationToken).ConfigureAwait(false);
        var scanned = new List<AtariScannedFirmware>(candidates.Count);
        foreach (var path in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            scanned.Add(await AtariFirmwareScanFunctions.ScanFileAsync(path, model, region, cancellationToken)
                .ConfigureAwait(false));
        }

        var duplicateHashes = scanned.Where(item => item.Md5 is not null)
            .GroupBy(item => item.Md5!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() >= AtariFirmwareConstants.DuplicateMinimumCount)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return scanned.Select(item => item with
            { IsDuplicate = item.Md5 is not null && duplicateHashes.Contains(item.Md5) }).ToArray();
    }

}
