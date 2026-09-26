namespace GWGUI.Emulation.Amstrad.Common.Constants;


internal static class ConfigurationStoreConstants
{
    internal const string MachineFileName = "machine.json";
    internal const string JsonSearchPattern = "*.json";
    internal const string TemporaryFileSuffix = ".tmp";
    internal const string LegacyFileExtension = ".json";
    internal const string MachineIdentifierFormat = "N";
    internal const string ParentDirectoryName = "..";
    internal const char StoredDirectorySeparator = '/';
    internal const int WriteBufferSize = 4096;
    internal const int CurrentSchemaVersion = 1;
    internal const int MinimumSchemaVersion = 1;
    internal const string TemporaryNameSeparator = ".";
    internal const int InitialWriterCount = 1;
    internal const int MaximumWriterCount = 1;
    internal const bool WriteIndentedJson = true;
    internal const bool UseAsyncFileAccess = true;
    internal const bool RecursiveDirectoryDelete = true;
}
