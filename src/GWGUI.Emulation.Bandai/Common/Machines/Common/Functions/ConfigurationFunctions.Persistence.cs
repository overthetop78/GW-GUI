using GWGUI.Emulation.Bandai.Common.Constants;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Enums;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class ConfigurationStoreFunctions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = ConfigurationStoreConstants.WriteIndentedJson
    };

    internal static ConfigurationDocument Deserialize(JsonElement root) => root.Deserialize<ConfigurationDocument>(JsonOptions) ?? throw new InvalidDataException();

    internal static ConfigurationDocument ToDocument(MachineConfiguration configuration, string pathBase) => new(
        ConfigurationStoreConstants.CurrentSchemaVersion,
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
        configuration.AudioEnabled);

    internal static MachineConfiguration FromDocument(ConfigurationDocument document, string pathBase)
    {
        if (document.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new InvalidDataException();
        if (document.Id == Guid.Empty)
            throw new InvalidDataException();
        if (document.Firmwares is null || document.Media is null || document.Options is null
           || document.Input is null || document.Folders is null)
            throw new InvalidDataException();
        if (!Enum.IsDefined(document.Model) || !Enum.IsDefined(document.Core))
            throw new InvalidDataException();
        return new MachineConfiguration(document.Model,
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
            document.SchemaVersion,
            document.AudioEnabled,
            ResolveFolders(document.Folders, pathBase),
            document.Core);
    }

    internal static string? StorePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var fullPath = Path.GetFullPath(path);
        var relative = Path.GetRelativePath(pathBase, fullPath);
        return IsInsideBase(relative) ? relative.Replace(Path.DirectorySeparatorChar, ConfigurationStoreConstants.StoredDirectorySeparator) : fullPath;
    }

    internal static string? ResolvePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathFullyQualified(path)) return path;
        return Path.GetFullPath(Path.Combine(pathBase, path.Replace(ConfigurationStoreConstants.StoredDirectorySeparator, Path.DirectorySeparatorChar)));
    }

    internal static async Task WriteDocumentAtomicallyAsync(string path, ConfigurationDocument document, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        var temporaryPath = fullPath + ConfigurationStoreConstants.TemporaryNameSeparator + Guid.NewGuid().ToString(ConfigurationStoreConstants.MachineIdentifierFormat) + ConfigurationStoreConstants.TemporaryFileSuffix;
        await ConfigurationFileAccessFunctions.WriteAtomicallyAsync(fullPath, temporaryPath, async temporary =>
        {
            await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, ConfigurationStoreConstants.WriteBufferSize, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, document, JsonOptions, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    private static bool IsInsideBase(string relative) =>
        !Path.IsPathFullyQualified(relative)
        && !string.Equals(relative, ConfigurationStoreConstants.ParentDirectoryName, StringComparison.Ordinal)
        && !relative.StartsWith(ConfigurationStoreConstants.ParentDirectoryName + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        && !relative.StartsWith(ConfigurationStoreConstants.ParentDirectoryName + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);

    private static FolderConfiguration StoreFolders(FolderConfiguration folders, string pathBase) =>
        new(StorePath(folders.Shared, pathBase), StorePath(folders.Cartridges, pathBase), StorePath(folders.CompactDiscs, pathBase), StorePath(folders.Saves, pathBase), StorePath(folders.States, pathBase), StorePath(folders.Captures, pathBase));

    private static FolderConfiguration ResolveFolders(FolderConfiguration folders, string pathBase) =>
        new(ResolvePath(folders.Shared, pathBase), ResolvePath(folders.Cartridges, pathBase),
            ResolvePath(folders.CompactDiscs, pathBase), ResolvePath(folders.Saves, pathBase), ResolvePath(folders.States, pathBase), ResolvePath(folders.Captures, pathBase));
}
