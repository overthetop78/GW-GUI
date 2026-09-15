namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMovieMakerMovie(IReadOnlyList<byte>? data)
    {
        ReadOnlySpan<byte> terminalRecord = [0xc0, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00];

        if (data is not { Count: > 10 }
            || data[0] != 0x01
            || data[1] != 0x00
            || data[2] != 0x01)
        {
            return false;
        }

        var terminalOffset = data.Count - terminalRecord.Length;
        for (var index = 0; index < terminalRecord.Length; index++)
        {
            if (data[terminalOffset + index] != terminalRecord[index])
                return false;
        }

        var terminalCount = 0;
        for (var offset = 3; offset <= terminalOffset; offset++)
        {
            var matches = true;
            for (var index = 0; index < terminalRecord.Length; index++)
            {
                if (data[offset + index] == terminalRecord[index])
                    continue;

                matches = false;
                break;
            }

            if (matches)
                terminalCount++;
        }

        return terminalCount == 1;
    }
}
