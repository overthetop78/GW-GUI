using GWGUI.MediaEngine.Migration;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Valide les noms ProDOS sans les transformer silencieusement.</summary>
public sealed class ProDosNamePolicy : IMigrationNamePolicy
{
    /// <inheritdoc />
    public bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length <= ProDosFileSystemLayout.MaximumNameLength && name[0] is >= 'A' and <= 'Z' && name.Skip(1).All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '.');
}
