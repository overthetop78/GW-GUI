using System.IO;
using GWGUI.Emulation.Snk.Common.Exceptions;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.Tests.Emulation.Snk;

public sealed class SnkFbAlphaCcdTests
{
    [Theory]
    [InlineData(0x01)]
    [InlineData(0x99)]
    public void TrackNumbersInsideNativeTableRemainSupported(byte track) => CcdRuntimeFunctions.ValidateSubchannelRecord(Record(track));

    [Theory]
    [InlineData(0x00)]
    [InlineData(0xA0)]
    [InlineData(0xAA)]
    [InlineData(0xFF)]
    public void TrackNumbersOutsideNativeTableAreRejected(byte track) => Assert.Throws<EmulationException>(() => CcdRuntimeFunctions.ValidateSubchannelRecord(Record(track)));

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void RecordsNotIndexedByNativeReaderRemainAllowed(byte index, byte control)
    {
        var record = Record(0xAA);
        record[RuntimeOptionConstants.SubchannelIndexOffset] = index;
        record[RuntimeOptionConstants.SubchannelQOffset] = control;
        CcdRuntimeFunctions.ValidateSubchannelRecord(record);
    }

    [Fact]
    public void AllSubchannelRecordsAreCheckedAndBorrowedStreamStaysOpen()
    {
        using var stream = new MemoryStream([.. Record(0x01), .. Record(0xAA)]);
        Assert.Throws<EmulationException>(() => CcdRuntimeFunctions.ValidateSubchannels(stream));
        Assert.True(stream.CanRead);
    }

    private static byte[] Record(byte track)
    {
        var record = new byte[OpticalSectorConstants.SubchannelSize];
        record[RuntimeOptionConstants.SubchannelQOffset] = 1;
        record[RuntimeOptionConstants.SubchannelTrackOffset] = track;
        record[RuntimeOptionConstants.SubchannelIndexOffset] = 1;
        return record;
    }

    [Fact]
    public void CompleteImageAndSubchannelSectorsRemainSupported() => CcdRuntimeFunctions.ValidateSizes(OpticalSectorConstants.RawSectorSize * 150L, OpticalSectorConstants.SubchannelSize * 150L);

    [Theory]
    [InlineData(0, 96)]
    [InlineData(2352, 0)]
    [InlineData(2351, 96)]
    [InlineData(2352, 95)]
    [InlineData(2352, 2147483616)]
    public void InvalidNativeDimensionsAreRejected(long imageBytes, long subchannelBytes) => Assert.Throws<EmulationException>(() => CcdRuntimeFunctions.ValidateSizes(imageBytes, subchannelBytes));
}
