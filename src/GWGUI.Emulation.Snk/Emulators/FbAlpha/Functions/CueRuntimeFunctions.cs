using System.IO;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;

internal static class CueRuntimeFunctions
{
    internal static IEmulationCueSheet Prepare(IEmulationCueSheet sheet, string stagedBinary, string stagedCue)
    {
        Validate(sheet);
        return CommonCoreHostFunctions.MediaProcessing.WithSingleCueFile(sheet, Path.GetRelativePath(Path.GetDirectoryName(stagedCue)!, stagedBinary));
    }

    internal static void Validate(IEmulationCueSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        if (!CommonCoreHostFunctions.MediaProcessing.IsSingleBinaryRawAudioCue(sheet, RuntimeOptionConstants.NativeMaximumTrackNumber)) throw Unsupported();
    }

    private static EmulationException Unsupported() => new(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
}