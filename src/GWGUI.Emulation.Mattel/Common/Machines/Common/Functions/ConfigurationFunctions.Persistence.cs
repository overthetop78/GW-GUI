using GWGUI.Emulation.Mattel.Common.Constants;
using GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Mattel.Common.Machines.Common.Enums;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

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
        configuration.AudioEnabled, configuration.ConfiguredSlots);

    internal static MachineConfiguration FromDocument(ConfigurationDocument document, string pathBase)
    {
        if (document.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new InvalidDataException();
        if (document.Id == Guid.Empty)
            throw new InvalidDataException();
        if (document.Firmwares is null || document.Media is null || document.Options is null
            || document.Input is null || document.Folders is null || document.ConfiguredSlots is null)
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
            document.Core, document.ConfiguredSlots);
    }

    internal static string? StorePath(string? path, string pathBase)
        => ConfigurationFileAccessFunctions.StorePath(path, pathBase);

    internal static string? ResolvePath(string? path, string pathBase)
        => ConfigurationFileAccessFunctions.ResolvePath(path, pathBase);

    internal static Task WriteDocumentAtomicallyAsync(string path, ConfigurationDocument document, CancellationToken cancellationToken)
        =>
            ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(path, document, JsonOptions, cancellationToken);

    private static FolderConfiguration StoreFolders(FolderConfiguration folders, string pathBase) => new(StorePath(folders.Shared, pathBase), StorePath(folders.Cartridges, pathBase), StorePath(folders.Saves, pathBase), StorePath(folders.States, pathBase), StorePath(folders.Captures, pathBase));

    private static FolderConfiguration ResolveFolders(FolderConfiguration folders, string pathBase) =>
        new(ResolvePath(folders.Shared, pathBase), ResolvePath(folders.Cartridges, pathBase), ResolvePath(folders.Saves, pathBase), ResolvePath(folders.States, pathBase), ResolvePath(folders.Captures, pathBase));
}
