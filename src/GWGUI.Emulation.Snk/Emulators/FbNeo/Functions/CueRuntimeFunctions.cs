using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;
using GWGUI.MediaEngine.Images.Reading.Sources;

namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Functions;

internal static class CueRuntimeFunctions
{
    private const int CopyBufferBytes = 64 * 1024;

    internal static void Prepare(string sourceCue, string stagedCue, SessionContentDirectory directory) => PrepareAsync(sourceCue, stagedCue, directory).GetAwaiter().GetResult();

    private static async Task PrepareAsync(string sourceCue, string stagedCue, SessionContentDirectory directory)
    {
        var sheet = new CueSheetReader().Parse(File.ReadLines(sourceCue));
        var sourceDirectory = Path.GetDirectoryName(sourceCue)!;
        var stagedDirectory = Path.GetDirectoryName(stagedCue)!;
        var files = new List<CueFileDescriptor>();
        var converted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in sheet.Files)
        {
            var original = Path.GetFullPath(file.DeclaredPath, sourceDirectory);
            var staged = directory.AddFile(original);
            if (file.Kind == CueFileKind.WavePcm && converted.Add(staged))
            {
                if (file.Tracks.Any(track => track.Mode != OpticalTrackMode.Audio)) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
                var pcm = WavePcmRandomAccessData.Open(original, requireCompleteSectors: false);
                using var output = new FileStream(staged, FileMode.Create, FileAccess.Write, FileShare.None);
                var buffer = new byte[CopyBufferBytes];
                for (long offset = 0; offset < pcm.Length;)
                {
                    var count = (int)Math.Min(buffer.Length, pcm.Length - offset);
                    await pcm.ReadExactlyAsync(offset, buffer.AsMemory(0, count)).ConfigureAwait(false);
                    await output.WriteAsync(buffer.AsMemory(0, count)).ConfigureAwait(false);
                    offset += count;
                }
                var padding = (int)((OpticalSectorConstants.AudioSectorSize - pcm.Length % OpticalSectorConstants.AudioSectorSize) % OpticalSectorConstants.AudioSectorSize);
                if (padding > 0) await output.WriteAsync(new byte[padding]).ConfigureAwait(false);
            }
            files.Add(new CueFileDescriptor(Path.GetRelativePath(stagedDirectory, staged), file.Kind == CueFileKind.WavePcm ? CueFileKind.Binary : file.Kind, file.Tracks));
        }
        var prepared = new CueSheetDocument(files, sheet.CatalogNumber, sheet.Metadata);
        using var cue = new FileStream(stagedCue, FileMode.Create, FileAccess.Write, FileShare.None);
        await new CueSheetWriter().WriteAsync(prepared, cue).ConfigureAwait(false);
    }
}
