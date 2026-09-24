using System.Globalization;

namespace Hst.Amiga.FileSystems.FastFileSystem;

internal static class FastFileSystemErrorMessages
{
    internal const string ReadFailed = "The entry could not be read.";
    internal const string CountExceedsBufferTemplate =
        "Count '{0}' is larger than buffer size '{1}'.";
    internal const string IncorrectDataBlockSequence =
        "The next file block has an incorrect sequence number.";
    internal const string NoFreeSectorAvailable = "No more free sectors are available.";
    internal const string OnlyOffsetZeroSupported = "Only offset 0 is supported.";
    internal const string SetLengthUnsupported =
        "Entry streams do not support SetLength; write a buffer with the required length instead.";

    internal static string CountExceedsBuffer(int count, int bufferLength) =>
        string.Format(CultureInfo.InvariantCulture, CountExceedsBufferTemplate, count, bufferLength);
}
