using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Thomson.Common.Constants;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;
using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
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
        configuration.Media.Select(media => media with { Path = StorePath(media.Path, pathBase)! }).ToArray(),
        new Dictionary<string, string>(configuration.Options),
        configuration.Input,
        configuration.AudioEnabled,
        configuration.ConfiguredSlots,
        StorePath(configuration.InitialContent, pathBase)
    );
    internal static MachineConfiguration FromDocument(ConfigurationDocument document, string pathBase)
    {
        if (document.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || document.Id == Guid.Empty)
            throw new InvalidDataException();
        if (document.Media is null || document.Options is null || document.Input is null || document.ConfiguredSlots is null)
            throw new InvalidDataException();
        if (!Enum.IsDefined(document.Model) || !Enum.IsDefined(document.Core))
            throw new InvalidDataException();
        return new MachineConfiguration(
            document.Model,
            document.Media.Select(media => media with { Path = ResolvePath(media.Path, pathBase)! }).ToArray(),
            document.Options,
            document.Input,
            document.Id,
            document.SchemaVersion,
            document.AudioEnabled,
            document.Core,
            document.ConfiguredSlots,
            ResolvePath(document.InitialContent, pathBase)
        );
    }

    internal static string? StorePath(string? path, string pathBase)
        => ConfigurationFileAccessFunctions.StorePath(path, pathBase);

    internal static string? ResolvePath(string? path, string pathBase)
        => ConfigurationFileAccessFunctions.ResolvePath(path, pathBase);

    internal static Task WriteDocumentAtomicallyAsync(string path, ConfigurationDocument document, CancellationToken cancellationToken)
        =>
            ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(path, document, JsonOptions, cancellationToken);
}
