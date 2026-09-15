namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsProTrackerModule(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 1084 } ||
            data[1080] != (byte)'M' || data[1081] != (byte)'.' ||
            data[1082] != (byte)'K' || data[1083] != (byte)'.')
            return false;

        long sampleBytes = 0;
        for (var sample = 0; sample < 31; sample++)
        {
            var offset = 20 + sample * 30;
            sampleBytes += (data[offset + 22] << 8 | data[offset + 23]) * 2L;
            if (data[offset + 24] > 0x0f || data[offset + 25] > 64) return false;
        }

        var orderCount = data[950];
        if (orderCount is < 1 or > 128) return false;
        var maximumPattern = 0;
        for (var index = 0; index < orderCount; index++)
        {
            var pattern = data[952 + index];
            if (pattern > 127) return false;
            maximumPattern = Math.Max(maximumPattern, pattern);
        }

        var expectedLength = 1084L + (maximumPattern + 1L) * 1024L + sampleBytes;
        return expectedLength == data.Count;
    }
}
