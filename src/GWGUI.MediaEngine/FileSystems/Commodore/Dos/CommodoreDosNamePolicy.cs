using GWGUI.MediaFileSystems.Primitives;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Conversion.Migration;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Valide les noms représentables dans les champs PETSCII Commodore DOS.</summary>
public sealed class CommodoreDosNamePolicy : IMigrationNamePolicy
{
    /// <inheritdoc />
    public bool IsValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > CommodoreDosLayout.NameLength) return false;
        try
        {
            _ = PetsciiCodec.Encode(name, CommodoreDosLayout.NameLength);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
