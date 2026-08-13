using GWGUI.MediaEngine.Migration;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

public sealed class Fat12ShortNamePolicy : IMigrationNamePolicy
{
    private const string AllowedPunctuation = "$%'-_@~`!(){}^#&";

    public bool IsValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name is FatDirectoryLayout.CurrentDirectoryName or FatDirectoryLayout.ParentDirectoryName) return false;
        var parts = name.Split(FatDirectoryLayout.ExtensionSeparator);
        return parts.Length <= 2 && parts[0].Length is > 0 and <= FatDirectoryLayout.NameLength && (parts.Length == 1 || parts[1].Length <= FatDirectoryLayout.ExtensionLength) && parts.All(part => part.All(IsValidCharacter));
    }

    private static bool IsValidCharacter(char value) => value is >= 'A' and <= 'Z' or >= '0' and <= '9' || AllowedPunctuation.Contains(value);
}
