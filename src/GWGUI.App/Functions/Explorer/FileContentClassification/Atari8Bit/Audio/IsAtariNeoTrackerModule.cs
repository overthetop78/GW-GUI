namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariNeoTrackerModule(IReadOnlyList<byte>? data)
    {
        const int fixedLength = 39553;
        const int titleOffset = 6;
        const int titleLength = 40;
        const int firstSampleNameOffset = 176;
        const int sampleNameLength = 16;
        const int confirmedSampleNameCount = 32;

        if (data is null
            || data.Count < fixedLength + 1
            || data[0] != (byte)'N'
            || data[1] != (byte)'E'
            || data[2] != (byte)'O'
            || data[3] != 0
            || data[4] != 0x10
            || data[5] != 0x8e
            || data[46] == 0
            || data[46] > 0x0f)
        {
            return false;
        }

        for (var offset = titleOffset; offset < titleOffset + titleLength; offset++)
        {
            if (data[offset] != 0 && data[offset] is < 0x20 or > 0x7e)
                return false;
        }

        var positionCount = (data[47] & 0x7f) + 1;
        if (data.Count != fixedLength + positionCount || data[48] >= positionCount)
            return false;

        var sampleNamesOffset = firstSampleNameOffset + positionCount;
        var sampleNamesEnd = sampleNamesOffset + confirmedSampleNameCount * sampleNameLength;
        if (sampleNamesEnd > data.Count)
            return false;

        for (var offset = sampleNamesOffset; offset < sampleNamesEnd; offset++)
        {
            if (data[offset] != 0 && data[offset] is < 0x20 or > 0x7e)
                return false;
        }

        return true;
    }
}
