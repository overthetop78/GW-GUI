namespace GWGUI.Emulation.Atari.Common.Services;

public sealed class FirmwareScanner(string firmwareRoot)
{
    private readonly string _firmwareRoot = Path.GetFullPath(firmwareRoot);

    public async Task<IReadOnlyList<ScannedFirmware>> ScanAsync(MachineModel model,
        StRegion? region = null, CancellationToken cancellationToken = default)
    {
        var candidates = await Task.Run(() => FirmwareScanFunctions.EnumerateCandidates(_firmwareRoot),
            cancellationToken).ConfigureAwait(false);
        var scanned = new List<ScannedFirmware>(candidates.Count);
        foreach (var path in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            scanned.Add(await FirmwareScanFunctions.ScanFileAsync(path, model, region, cancellationToken)
                .ConfigureAwait(false));
        }

        var duplicateHashes = scanned.Where(item => item.Md5 is not null)
            .GroupBy(item => item.Md5!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() >= FirmwareConstants.DuplicateMinimumCount)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return scanned.Select(item => item with
            { IsDuplicate = item.Md5 is not null && duplicateHashes.Contains(item.Md5) }).ToArray();
    }

}
