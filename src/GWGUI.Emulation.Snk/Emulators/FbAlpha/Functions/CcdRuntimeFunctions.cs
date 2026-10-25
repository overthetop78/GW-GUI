using System.IO;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;
internal static class CcdRuntimeFunctions
{
    internal static void ValidateSubchannels(Stream subchannel)
    {
        Span<byte> sector = stackalloc byte[CommonCoreHostFunctions.MediaProcessing.OpticalSubchannelSize];
        while (subchannel.Position < subchannel.Length)
        {
            subchannel.ReadExactly(sector);
            ValidateSubchannelRecord(sector);
        }
    }

    internal static void ValidateSubchannelRecord(ReadOnlySpan<byte> sector)
    {
        if (sector.Length != CommonCoreHostFunctions.MediaProcessing.OpticalSubchannelSize)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
        if (sector[RuntimeOptionConstants.SubchannelIndexOffset] == 0 || (sector[RuntimeOptionConstants.SubchannelQOffset] & RuntimeOptionConstants.SubchannelAddressMask) == 0)
            return;
        var encoded = sector[RuntimeOptionConstants.SubchannelTrackOffset];
        var track = (encoded >> RuntimeOptionConstants.BcdDigitBits) * RuntimeOptionConstants.BcdDecimalBase + (encoded & RuntimeOptionConstants.BcdLowDigitMask);
        if (track < 1 || track > RuntimeOptionConstants.NativeMaximumTrackNumber)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
    }

    internal static void ValidateSizes(long imageBytes, long subchannelBytes)
    {
        if (imageBytes <= 0 || imageBytes % CommonCoreHostFunctions.MediaProcessing.RawOpticalSectorSize != 0 || subchannelBytes <= 0 || subchannelBytes % CommonCoreHostFunctions.MediaProcessing.OpticalSubchannelSize != 0 || subchannelBytes > int.MaxValue - (CommonCoreHostFunctions.MediaProcessing.OpticalSubchannelSize - 1))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
    }
}