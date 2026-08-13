using GWGUI.MediaEngine.Migration;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Valide les noms représentables dans un catalogue Apple DOS.</summary>
public sealed class AppleDosNamePolicy : IMigrationNamePolicy
{
    /// <inheritdoc />
    public bool IsValid(string name) => name.Length is > 0 and <= AppleDosFileSystemLayout.EntryNameLength && name[0] is >= '@' and <= '~' && name[^1] != ' ' && name.All(character => character is >= ' ' and <= '~' && character != ',');
}
