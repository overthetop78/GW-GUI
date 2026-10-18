using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Wenquxing.Common.Constants;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
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
        new FirmwareConfiguration(configuration.Firmware.Images.ToDictionary(image => image.Key, image => StorePath(image.Value, pathBase)!)),
        configuration.Media.Select(media => media with { Path = StorePath(media.Path, pathBase)! }).ToArray(),
        new Dictionary<string, string>(configuration.Options),
        configuration.Input,
        configuration.AudioEnabled,
        configuration.ConfiguredSlots
    );
    internal static MachineConfiguration FromDocument(ConfigurationDocument document, string pathBase)
    {
        if (document.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || document.Id == Guid.Empty)
            throw new InvalidDataException();
        if (document.Firmware is null || document.Firmware.Images is null || document.Media is null || document.Options is null || document.Input is null || document.ConfiguredSlots is null)
            throw new InvalidDataException();
        if (!Enum.IsDefined(document.Model) || !Enum.IsDefined(document.Core))
            throw new InvalidDataException();
        return new MachineConfiguration(
            document.Model,
            new FirmwareConfiguration(document.Firmware.Images.ToDictionary(image => image.Key, image => ResolvePath(image.Value, pathBase)!)),
            document.Media.Select(media => media with { Path = ResolvePath(media.Path, pathBase)! }).ToArray(),
            document.Options,
            document.Input,
            document.Id,
            document.SchemaVersion,
            document.AudioEnabled,
            document.Core,
            document.ConfiguredSlots
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
