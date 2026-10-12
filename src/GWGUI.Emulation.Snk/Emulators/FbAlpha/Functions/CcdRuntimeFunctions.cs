using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;
internal static class CcdRuntimeFunctions
{
    internal static void ValidateSubchannels(Stream subchannel)
    {
        Span<byte> sector = stackalloc byte[OpticalSectorConstants.SubchannelSize];
        while (subchannel.Position < subchannel.Length)
        {
            subchannel.ReadExactly(sector);
            ValidateSubchannelRecord(sector);
        }
    }

    internal static void ValidateSubchannelRecord(ReadOnlySpan<byte> sector)
    {
        if (sector.Length != OpticalSectorConstants.SubchannelSize)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
        if (sector[RuntimeOptionConstants.SubchannelIndexOffset] == 0 || (sector[RuntimeOptionConstants.SubchannelQOffset] & RuntimeOptionConstants.SubchannelAddressMask) == 0)
            return;
        var encoded = sector[RuntimeOptionConstants.SubchannelTrackOffset];
        var track = (encoded >> RuntimeOptionConstants.BcdDigitBits) * RuntimeOptionConstants.BcdDecimalBase + (encoded & RuntimeOptionConstants.BcdLowDigitMask);
        if (track < 1 || track > RuntimeOptionConstants.NativeMaximumTrackNumber)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
    }

    internal static void ValidateSizes(
        long imageBytes,
        long subchannelBytes
    )
    {
        if (imageBytes <= 0 || imageBytes % OpticalSectorConstants.RawSectorSize != 0 || subchannelBytes <= 0 || subchannelBytes % OpticalSectorConstants.SubchannelSize != 0 || subchannelBytes > int.MaxValue - (OpticalSectorConstants.SubchannelSize - 1))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
    }
}