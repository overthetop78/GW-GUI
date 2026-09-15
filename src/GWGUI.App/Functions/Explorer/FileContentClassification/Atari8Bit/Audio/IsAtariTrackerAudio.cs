using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariTrackerAudio(FileSystemEntry entry)
    {
        if (entry.Content is not { } data)
            return false;

        var extension = System.IO.Path.GetExtension(entry.Name);
        if (extension.Equals(".mpt", StringComparison.OrdinalIgnoreCase))
            return IsMusicProTrackerModule(data);

        if (extension.Equals(".md1", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".md2", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".tmc", StringComparison.OrdinalIgnoreCase))
        {
            return data.Count >= 32 && data[0] == 0xff && data[1] == 0xff;
        }

        return (extension.Equals(".d15", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".d8", StringComparison.OrdinalIgnoreCase))
            && IsMusicProTrackerSampleBank(data);
    }

    private static bool IsMusicProTrackerModule(IReadOnlyList<byte> data)
    {
        if (data.Count < 0x1d0 || data[0] != 0xff || data[1] != 0xff) return false;

        static int Word(IReadOnlyList<byte> bytes, int offset) => bytes[offset] | bytes[offset + 1] << 8;
        var startAddress = Word(data, 2);
        var endAddress = Word(data, 4);
        if (endAddress < startAddress) return false;

        var trackAddresses = new int[4];
        for (var channel = 0; channel < trackAddresses.Length; channel++)
            trackAddresses[channel] = data[0x1c6 + channel] | data[0x1ca + channel] << 8;
        var relocation = trackAddresses[0] - startAddress - 0x1ca;
        var relocatedStartAddress = startAddress + relocation;
        var relocatedEndAddress = endAddress + relocation;
        if (relocatedStartAddress is < 0 or > ushort.MaxValue
            || relocatedEndAddress is < 0 or > ushort.MaxValue)
            return false;
        for (var channel = 1; channel < trackAddresses.Length; channel++)
            if (trackAddresses[channel] <= trackAddresses[channel - 1]) return false;

        var firstDataAddress = relocatedEndAddress + 1;
        for (var offset = 6; offset < 0xc6; offset += 2)
        {
            var address = Word(data, offset);
            if (address == 0) continue;
            if (address < trackAddresses[3] || address > relocatedEndAddress) return false;
            firstDataAddress = address;
            break;
        }
        if (firstDataAddress <= trackAddresses[3]) return false;

        var firstTrackLength = trackAddresses[1] - trackAddresses[0];
        if ((firstTrackLength & 1) != 0 || firstTrackLength / 2 is <= 0 or > 0xfe) return false;
        var declaredLength = 7 + endAddress - startAddress;
        if (declaredLength == data.Count) return true;

        var shortSongAdjustment = 3 * firstTrackLength + trackAddresses[1] - firstDataAddress;
        return data.Count - shortSongAdjustment == declaredLength;
    }

    private static bool IsMusicProTrackerSampleBank(IReadOnlyList<byte> data)
    {
        if (data.Count is < 288 or > 12320 || data[0] == 0) return false;

        var firstPage = data[0];
        var previousEndPage = -1;
        var sampleCount = 0;
        for (var index = 0; index < 16; index++)
        {
            var startPage = data[index];
            var endPage = data[16 + index];
            if (startPage == 0)
            {
                for (; index < 16; index++)
                    if (data[index] != 0 || data[16 + index] != 0)
                        return false;
                break;
            }

            if (previousEndPage >= 0 && startPage != previousEndPage || endPage <= startPage)
                return false;

            previousEndPage = endPage;
            sampleCount++;
        }

        if (sampleCount == 0 || previousEndPage <= firstPage) return false;
        var requiredLength = 32 + (previousEndPage - firstPage) * 256;
        return data.Count >= requiredLength
            && data.Skip(32).Take(requiredLength - 32).Distinct().Take(4).Count() == 4;
    }
}
