using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariConfigurationStoreFunctions
{
    internal static AtariConfigurationDocument ToDocument(AtariMachineConfiguration configuration,
        string pathBase) => new(
        AtariConstants.CurrentConfigurationSchemaVersion,
        configuration.Id,
        configuration.Model,
        configuration.Core,
        configuration.Firmwares.Select(firmware => firmware with
        {
            Path = StorePath(firmware.Path, pathBase)!
        }).ToArray(),
        configuration.Media.Select(media => media with
        {
            Path = StorePath(media.Path, pathBase)!
        }).ToArray(),
        new Dictionary<string, string>(configuration.Options),
        configuration.Input,
        StoreFolders(configuration.Folders, pathBase),
        configuration.AudioEnabled,
        configuration.VideoRenderer,
        configuration.VideoProcessing);

    internal static AtariMachineConfiguration FromDocument(AtariConfigurationDocument document,
        string pathBase)
    {
        if (document.Id == Guid.Empty)
            throw new InvalidDataException(AtariConfigurationStoreConstants.InvalidIdentifierError);
        if (document.Firmwares is null || document.Media is null || document.Options is null
            || document.Input is null || document.Folders is null)
            throw new InvalidDataException(AtariConfigurationStoreConstants.InvalidDocumentError);
        if (AtariConfigurationFunctions.GetCore(document.Model) != document.Core)
            throw new InvalidDataException(AtariConfigurationStoreConstants.CoreMismatchError);
        return new AtariMachineConfiguration(document.Model,
            document.Firmwares.Select(firmware => firmware with
            {
                Path = ResolvePath(firmware.Path, pathBase)!
            }).ToArray(),
            document.Media.Select(media => media with
            {
                Path = ResolvePath(media.Path, pathBase)!
            }).ToArray(),
            document.Options,
            document.Input,
            document.Id,
            AtariConstants.CurrentConfigurationSchemaVersion,
            document.AudioEnabled,
            document.VideoRenderer,
            ResolveFolders(document.Folders, pathBase),
            document.VideoProcessing);
    }

    internal static string? StorePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var fullPath = Path.GetFullPath(path);
        var relative = Path.GetRelativePath(pathBase, fullPath);
        return IsInsideBase(relative)
            ? relative.Replace(Path.DirectorySeparatorChar,
                AtariConfigurationStoreConstants.StoredDirectorySeparator)
            : fullPath;
    }

    internal static string? ResolvePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathFullyQualified(path)) return path;
        return Path.GetFullPath(Path.Combine(pathBase,
            path.Replace(AtariConfigurationStoreConstants.StoredDirectorySeparator,
                Path.DirectorySeparatorChar)));
    }

    internal static async Task WriteDocumentAtomicallyAsync(string path, AtariConfigurationDocument document,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N")
            + AtariConfigurationStoreConstants.TemporaryFileSuffix;
        try
        {
            await using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write,
                             FileShare.None, AtariConfigurationStoreConstants.WriteBufferSize,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, document, AtariConfigurationStoreConstants.JsonOptions,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            ConfigurationFileAccessFunctions.ReplaceFile(temporaryPath, fullPath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private static bool IsInsideBase(string relative) =>
        !Path.IsPathFullyQualified(relative)
        && !string.Equals(relative, AtariConfigurationStoreConstants.ParentDirectoryName, StringComparison.Ordinal)
        && !relative.StartsWith(AtariConfigurationStoreConstants.ParentDirectoryName + Path.DirectorySeparatorChar,
            StringComparison.Ordinal)
        && !relative.StartsWith(AtariConfigurationStoreConstants.ParentDirectoryName + Path.AltDirectorySeparatorChar,
            StringComparison.Ordinal);

    private static AtariFolderConfiguration StoreFolders(AtariFolderConfiguration folders, string pathBase) =>
        new(StorePath(folders.Shared, pathBase), StorePath(folders.Floppies, pathBase),
            StorePath(folders.Cassettes, pathBase), StorePath(folders.Cartridges, pathBase),
            StorePath(folders.CompactDiscs, pathBase), StorePath(folders.HardDisks, pathBase),
            StorePath(folders.States, pathBase), StorePath(folders.Captures, pathBase));

    private static AtariFolderConfiguration ResolveFolders(AtariFolderConfiguration folders, string pathBase) =>
        new(ResolvePath(folders.Shared, pathBase), ResolvePath(folders.Floppies, pathBase),
            ResolvePath(folders.Cassettes, pathBase), ResolvePath(folders.Cartridges, pathBase),
            ResolvePath(folders.CompactDiscs, pathBase), ResolvePath(folders.HardDisks, pathBase),
            ResolvePath(folders.States, pathBase), ResolvePath(folders.Captures, pathBase));
}
