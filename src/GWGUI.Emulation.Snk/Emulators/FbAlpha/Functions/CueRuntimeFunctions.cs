using System.IO;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;

internal static class CueRuntimeFunctions
{
    internal static CueSheetDocument Prepare(CueSheetDocument sheet, string stagedBinary, string stagedCue)
    {
        Validate(sheet);
        var file = sheet.Files[0];
        return new CueSheetDocument([new CueFileDescriptor(Path.GetRelativePath(Path.GetDirectoryName(stagedCue)!, stagedBinary), file.Kind, file.Tracks)], sheet.CatalogNumber, sheet.Metadata);
    }

    internal static void Validate(CueSheetDocument sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        if (sheet.Files.Count != 1 || sheet.Files[0].Kind != CueFileKind.Binary) throw Unsupported();
        var tracks = sheet.Files[0].Tracks;
        if (tracks.Count == 0 || tracks.Count > RuntimeOptionConstants.NativeMaximumTrackNumber) throw Unsupported();
        for (var index = 0; index < tracks.Count; index++)
        {
            var track = tracks[index];
            if (track.Number != index + 1 || track.Mode is not (OpticalTrackMode.Mode1Raw2352 or OpticalTrackMode.Audio) || !track.Indexes.Any(position => position.Number == 1)) throw Unsupported();
        }
    }

    private static EmulationException Unsupported() => new(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
}
