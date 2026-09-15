using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPrexorRoutine(IReadOnlyList<byte>? data) =>
        data is { Count: 328 }
        && data.Take(64).All(value => value == 0)
        && ContainsSequence(data, AtariPrexorRoutineSignature)
        && data.Skip(data.Count - 32).All(value => value == 0x60);
}
