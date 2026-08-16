using System.Text.Json;

namespace GWGUI.Emulation.Atari;

internal static class AtariStateConstants
{
    internal const string MagicText = "GWATARI1";
    internal const string TemporaryFileSuffix = ".tmp";
    internal const int CurrentFormatVersion = 1;
    internal const int HeaderLengthSize = sizeof(int);
    internal const int MaximumHeaderLength = 1024 * 1024;
    internal const int HashBufferSize = 64 * 1024;
    internal const int EmptyLength = 0;
    internal const int FirstBufferIndex = 0;
    internal const int ReadRetryCount = 8;
    internal const int FirstRetryIndex = 0;
    internal const int NextRetryCount = 1;
    internal const char CanonicalDirectorySeparator = '/';
    internal const string AllFilesSearchPattern = "*";
    internal const string InvalidMagicError = "The file is not a GW GUI Atari state.";
    internal const string InvalidHeaderLengthError = "The Atari state header length is invalid.";
    internal const string InvalidHeaderError = "The Atari state header is invalid.";
    internal const string TruncatedFileError = "The Atari state file is truncated.";
    internal const string EmptyPayloadError = "The Atari state payload is empty.";
    internal const string PayloadTooLargeError = "The Atari state payload exceeds the supported size.";
    internal const string CorruptedPayloadError = "The Atari state payload is corrupted.";
    internal const string UnsupportedFormatError = "The Atari state format version is not supported.";
    internal const string CoreMismatchError = "The Atari state was created by a different emulator core.";
    internal const string ModelMismatchError = "The Atari state was created for a different Atari model.";
    internal const string ConfigurationMismatchError = "The Atari state configuration does not match the running machine.";
    internal const string ContentMismatchError = "The Atari state content does not match the running machine.";
    internal const string ContentPathMissingError = "The Atari state content path was not found.";
    internal const string FirmwareCategory = "firmware";
    internal const string MediaCategory = "media";
    internal static readonly byte[] Magic = System.Text.Encoding.ASCII.GetBytes(MagicText);
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
