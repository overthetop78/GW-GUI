using System.Security.Cryptography;

namespace GWGUI.Emulation.Atari;

public static class AtariFirmwareScanFunctions
{
    private static readonly IReadOnlySet<string> RelevantExtensions = new HashSet<string>(
        AtariFirmwareFunctions.Values(AtariFirmwareConstants.RomExtension, AtariFirmwareConstants.ImageExtension,
            AtariFirmwareConstants.TosExtension, AtariFirmwareConstants.BinaryExtension,
            AtariFirmwareConstants.JaguarExtension), StringComparer.OrdinalIgnoreCase);

    public static string FamilyDirectoryName(AtariMachineFamily family) => family switch
    {
        AtariMachineFamily.St => AtariFirmwareConstants.StFamilyDirectoryName,
        AtariMachineFamily.EightBit => AtariFirmwareConstants.EightBitFamilyDirectoryName,
        AtariMachineFamily.Atari5200 => AtariFirmwareConstants.Atari5200FamilyDirectoryName,
        AtariMachineFamily.Atari2600 => AtariFirmwareConstants.Atari2600FamilyDirectoryName,
        AtariMachineFamily.Atari7800 => AtariFirmwareConstants.Atari7800FamilyDirectoryName,
        AtariMachineFamily.Lynx => AtariFirmwareConstants.LynxFamilyDirectoryName,
        AtariMachineFamily.Jaguar => AtariFirmwareConstants.JaguarFamilyDirectoryName,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
    };

    public static IReadOnlyList<string> EnsureFamilyDirectories(string firmwareRoot)
    {
        var root = Path.GetFullPath(firmwareRoot);
        Directory.CreateDirectory(root);
        return Enum.GetValues<AtariMachineFamily>().Select(family =>
        {
            var directory = Path.Combine(root, FamilyDirectoryName(family));
            Directory.CreateDirectory(directory);
            return directory;
        }).ToArray();
    }

    public static bool IsRelevantFile(string path) => RelevantExtensions.Contains(Path.GetExtension(path)) ||
        AtariFirmwareCatalog.All.Any(definition => definition.ExpectedFileName is not null &&
            string.Equals(Path.GetFileName(path), definition.ExpectedFileName, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> EnumerateCandidates(string firmwareRoot)
    {
        var directories = EnsureFamilyDirectories(firmwareRoot);
        return directories.SelectMany(directory => Directory.EnumerateFiles(directory,
                AtariFirmwareConstants.AllFilesPattern, SearchOption.TopDirectoryOnly))
            .Where(IsRelevantFile)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static async Task<string> ComputeMd5Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            AtariFirmwareConstants.FileBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await MD5.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hash);
    }

    public static AtariFirmwareDefinition? Identify(string md5) => AtariFirmwareCatalog.All.FirstOrDefault(
        definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));

    public static AtariFirmwareDefinition? IdentifyByExpectedName(string path, AtariMachineModel model) =>
        AtariFirmwareCatalog.ForModel(model).FirstOrDefault(definition => definition.ExpectedFileName is not null &&
            string.Equals(Path.GetFileName(path), definition.ExpectedFileName, StringComparison.OrdinalIgnoreCase));

    public static AtariFirmwareCompatibility Classify(AtariFirmwareDefinition? identified,
        AtariFirmwareDefinition? named, AtariMachineModel model, AtariStRegion? region)
    {
        var candidate = identified ?? named;
        if (candidate is null || !candidate.Models.Contains(model)) return AtariFirmwareCompatibility.Incompatible;
        if (identified is null) return AtariFirmwareCompatibility.PartiallyCompatible;
        var fingerprintRegion = identified.Fingerprints.FirstOrDefault(fingerprint =>
            region is null || fingerprint.Region == region)?.Region;
        return region is not null && identified.Fingerprints.Any(fingerprint => fingerprint.Region is not null)
            && fingerprintRegion != region
            ? AtariFirmwareCompatibility.Incompatible
            : AtariFirmwareCompatibility.Compatible;
    }

    public static async Task<AtariScannedFirmware> ScanFileAsync(string path, AtariMachineModel model,
        AtariStRegion? region, CancellationToken cancellationToken)
    {
        try
        {
            var file = new FileInfo(path);
            var md5 = await ComputeMd5Async(path, cancellationToken).ConfigureAwait(false);
            var identified = Identify(md5);
            var named = IdentifyByExpectedName(path, model);
            return new(file.FullName, file.Length, md5,
                identified is null ? AtariFirmwareDetectionStatus.Unknown : AtariFirmwareDetectionStatus.Known,
                identified ?? named, Classify(identified, named, model, region), false, null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return new(Path.GetFullPath(path), null, null, AtariFirmwareDetectionStatus.Unreadable,
                IdentifyByExpectedName(path, model), AtariFirmwareCompatibility.Incompatible, false, error.Message);
        }
    }

    public static AtariFirmwareConfiguration CreateSelection(AtariScannedFirmware firmware)
    {
        if (firmware.Detection == AtariFirmwareDetectionStatus.Unreadable ||
            firmware.Compatibility == AtariFirmwareCompatibility.Incompatible || firmware.Definition?.Kind is null)
            throw new InvalidOperationException(AtariErrorMessages.FirmwareCannotBeSelected);
        return new(firmware.Definition.Kind.Value, firmware.Path, firmware.Definition.RequiresExternalFile);
    }
}
