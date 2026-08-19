using System.Globalization;

namespace GWGUI.Emulation.Atari;

public static class AtariFirmwareRuntimeFunctions
{
    public static void PrepareSystemDirectory(AtariMachineConfiguration configuration, string systemDirectory)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var absoluteSystemDirectory = Path.GetFullPath(systemDirectory);
        Directory.CreateDirectory(absoluteSystemDirectory);
        ValidateRequiredFirmware(configuration);
        ClearManagedFirmwareFiles(absoluteSystemDirectory);
        foreach (var firmware in configuration.Firmwares)
        {
            var sourcePath = Path.GetFullPath(firmware.Path);
            ValidateReadableFile(firmware.Kind, sourcePath);
            var definition = ResolveDefinition(configuration.Model, firmware.Kind, sourcePath);
            var targetPath = Path.Combine(absoluteSystemDirectory, definition.ExpectedFileName!);
            File.Copy(sourcePath, targetPath, true);
        }
    }

    public static void ClearManagedFirmwareFiles(string systemDirectory)
    {
        foreach (var fileName in AtariFirmwareCatalog.All.Select(definition => definition.ExpectedFileName)
                     .Where(fileName => fileName is not null).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var path = Path.Combine(systemDirectory, fileName!);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    public static void ValidateRequiredFirmware(AtariMachineConfiguration configuration)
    {
        var configuredKinds = configuration.Firmwares.Select(firmware => firmware.Kind).ToHashSet();
        var requiredKinds = AtariFirmwareCatalog.ForModel(configuration.Model)
            .Where(definition => definition.RequiresExternalFile && definition.Kind is not null)
            .Select(definition => definition.Kind!.Value)
            .Distinct();
        foreach (var kind in requiredKinds.Where(kind => !configuredKinds.Contains(kind)))
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                AtariErrorMessages.RequiredFirmwareMissing, kind, configuration.Model));
    }

    public static AtariFirmwareDefinition ResolveDefinition(AtariMachineModel model, AtariFirmwareKind kind,
        string sourcePath)
    {
        var definitions = AtariFirmwareCatalog.ForModel(model)
            .Where(definition => definition.Kind == kind && definition.ExpectedFileName is not null)
            .ToArray();
        var expectedFileNames = definitions.Select(definition => definition.ExpectedFileName!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (expectedFileNames.Length == AtariFirmwareRuntimeConstants.SingleDefinitionCount)
            return definitions[AtariFirmwareRuntimeConstants.FirstDefinitionIndex];

        var md5 = ComputeMd5(sourcePath);
        var identified = definitions.FirstOrDefault(definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));
        if (identified is not null) return identified;
        var named = definitions.FirstOrDefault(definition => string.Equals(definition.ExpectedFileName,
            Path.GetFileName(sourcePath), StringComparison.OrdinalIgnoreCase));
        return named ?? throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
            AtariErrorMessages.FirmwareIdentityAmbiguous, kind, sourcePath));
    }

    public static void ValidateReadableFile(AtariFirmwareKind kind, string sourcePath)
    {
        if (!File.Exists(sourcePath)) throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
            AtariErrorMessages.FirmwareFileMissing, kind, sourcePath), sourcePath);
        try
        {
            using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _ = stream.ReadByte();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException(string.Format(CultureInfo.InvariantCulture,
                AtariErrorMessages.FirmwareFileUnreadable, kind, sourcePath), error);
        }
    }

    private static string ComputeMd5(string sourcePath)
    {
        using var stream = File.OpenRead(sourcePath);
        return Convert.ToHexStringLower(System.Security.Cryptography.MD5.HashData(stream));
    }
}
