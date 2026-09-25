using System.Text.Json;

namespace GWGUI.Emulation.Atari.Common.Constants;

internal static class ConfigurationMigrationConstants
{
    internal const string SchemaVersionPropertyName = "schemaVersion";
}

public static class ConfigurationOptionConstants
{
    public const string VideoStandard = "gwgui_atari_video_standard";
    public const string VideoResolution = "gwgui_atari_video_resolution";
    public const string MainMemory = "gwgui_atari_main_memory";
    public const string AudioOutput = "gwgui_atari_audio_output";
    public const string AudioLatency = "gwgui_atari_audio_latency";
    public const string AudioVolume = "gwgui_atari_audio_volume";

    public const string DefaultAudioOutput = "default";
    public const int DefaultAudioLatencyMilliseconds = 50;
    public const int DefaultAudioVolumePercent = 100;
}

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

internal static class ConfigurationSummaryFunctionsConstants
{
    internal const string Value = " + ";
    internal const string D3D11 = "D3D11";
    internal const string AudioOn = "Audio On";
    internal const string AudioOff = "Audio Off";
    internal const string TOS = "TOS";
    internal const string Value2 = "…";
}
