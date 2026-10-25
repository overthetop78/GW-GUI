using System.IO;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Functions;

internal static class CueRuntimeFunctions
{
    private const int CopyBufferBytes = 64 * 1024;

    internal static void Prepare(string sourceCue, string stagedCue, SessionContentDirectory directory)
        => CommonCoreHostFunctions.MediaProcessing.PrepareCueWaveFilesAsync(sourceCue, stagedCue, path => directory.AddFile(path), static () => new InvalidDataException(ErrorMessages.IncompatibleMedia), CopyBufferBytes).GetAwaiter().GetResult();
}