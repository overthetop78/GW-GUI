using System.Text.Json;

namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariConfigurationStoreConstants
{
    internal const string MachineFileName = "machine.json";
    internal const string JsonSearchPattern = "*.json";
    internal const string TemporaryFileSuffix = ".tmp";
    internal const string LegacyFileExtension = ".json";
    internal const string MachineIdentifierFormat = "N";
    internal const string ParentDirectoryName = "..";
    internal const char StoredDirectorySeparator = '/';
    internal const int WriteBufferSize = 4096;
    internal const int NoActiveLoads = 0;
    internal const string EmptyDocumentError = "The Atari configuration document is empty.";
    internal const string UnsupportedSchemaError = "The Atari configuration schema version is not supported.";
    internal const string CoreMismatchError = "The stored Atari core does not match the selected model.";
    internal const string InvalidIdentifierError = "The Atari configuration identifier is invalid.";
    internal const string InvalidDocumentError = "The Atari configuration document is invalid.";
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
