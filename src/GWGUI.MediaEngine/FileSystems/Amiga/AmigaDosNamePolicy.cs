using GWGUI.MediaEngine.Migration;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

public sealed class AmigaDosNamePolicy : IMigrationNamePolicy
{
    public bool IsValid(string name) => name.Length is > 0 and <= AmigaDosLayout.OrdinaryNameMaximumLength && name.IndexOfAny(['/', ':']) < 0 && System.Text.Encoding.Latin1.GetString(System.Text.Encoding.Latin1.GetBytes(name)) == name;
}
