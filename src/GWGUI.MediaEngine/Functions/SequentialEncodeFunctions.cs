using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Functions;

internal static class SequentialEncodeFunctions
{
    internal static bool CanEncodeValidatedBytes(SequentialEncodeRequest request,
        IReadOnlySet<string> supportedFormats, IReadOnlySet<string> supportedMachines) =>
        supportedFormats.Contains(request.TargetFormatId)
        && supportedMachines.Contains(request.MachineId)
        && request.Blocks.Count > 0
        && request.Blocks.All(block => block.IntegrityValid == true && block.Data.Length > 0)
        && request.RetainedSegments.Count == 0
        && (!request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            || PcmSampleFunctions.TryReadSampleRate(request.Parameters, out _));
}
