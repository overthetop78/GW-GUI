namespace GWGUI.Domain.Naming;

public enum OutputConflictChoice { Overwrite, NextSequence, EditName }

public static class OutputConflictResolver
{
    public static string FindNextAvailable(string folder, string baseName, string extension, SequenceKind kind, int width, long start, Func<string, bool>? exists = null)
        => FindNextAvailableWithValue(folder, baseName, extension, kind, width, start, exists).Path;

    public static (string Path, long Value) FindNextAvailableWithValue(string folder, string baseName, string extension, SequenceKind kind, int width, long start, Func<string, bool>? exists = null)
    {
        exists ??= File.Exists;
        for (var value = start; value < long.MaxValue; value++)
        {
            var suffix = SequenceFormatter.Format(value, kind, width);
            var candidate = Path.Combine(folder, $"{baseName} {suffix}{extension}");
            if (!exists(candidate)) return (candidate, value);
        }
        throw new IOException("No available output name could be found.");
    }
}
