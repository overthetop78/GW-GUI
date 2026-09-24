using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Tzx;

/// <summary>Declares the shared TZX 1.20 container used with TZX, CDT, and TSX contexts.</summary>
internal static class TzxFormat
{
    public static string FormatId => TapeImageFormatIds.Tzx;
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Tzx, DiskImageFileExtensions.Cdt, DiskImageFileExtensions.Tsx }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<byte> SupportedBlockIds = new byte[]
    {
        TzxConstants.StandardSpeedData, TzxConstants.TurboSpeedData, TzxConstants.PureTone,
        TzxConstants.PulseSequence, TzxConstants.PureData, TzxConstants.DirectRecording,
        TzxConstants.CswRecording, TzxConstants.GeneralizedData, TzxConstants.Pause,
        TzxConstants.GroupStart, TzxConstants.GroupEnd, TzxConstants.Jump, TzxConstants.LoopStart,
        TzxConstants.LoopEnd, TzxConstants.CallSequence, TzxConstants.Return, TzxConstants.Select,
        TzxConstants.StopIf48K, TzxConstants.SetSignalLevel, TzxConstants.TextDescription,
        TzxConstants.Message, TzxConstants.ArchiveInfo, TzxConstants.HardwareType,
        TzxConstants.CustomInfo, TzxConstants.Glue
    }.ToFrozenSet();

    public static bool CanRead => true;
    public static bool CanWrite => true;
}
