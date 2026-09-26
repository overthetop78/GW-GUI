using System.Text.Json;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Constants;


internal static class StateConstants
{
    internal const string MagicText = "GWATARI1";
    internal const string TemporaryFileSuffix = ".tmp";
    internal const int CurrentFormatVersion = 1;
    internal const int HeaderLengthSize = sizeof(int);
    internal const int MaximumHeaderLength = 1024 * 1024;
    internal const int HashBufferSize = 64 * 1024;
    internal const int EmptyLength = 0;
    internal const int ReadRetryCount = 64;
    internal const int ReadRetryDelayMilliseconds = 1;
    internal const int FirstRetryIndex = 0;
    internal const int NextRetryCount = 1;
    internal const char CanonicalDirectorySeparator = '/';
    internal const string AllFilesSearchPattern = "*";
    internal const string FirmwareCategory = "firmware";
    internal const string MediaCategory = "media";
    internal static readonly byte[] Magic = System.Text.Encoding.ASCII.GetBytes(MagicText);
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal const string LegacyAudioProperty = "audioEnabled";
    internal const string LegacyPresentationProperty = "videoRenderer";
    internal const int LegacyPresentationValueCount = 4;
}
