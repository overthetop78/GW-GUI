using GWGUI.MediaEngine.Conversion.Migration;
using FileSystemsCommodoreDosNamePolicy = GWGUI.MediaFileSystems.FileSystems.Commodore.Dos.CommodoreDosNamePolicy;

namespace GWGUI.MediaEngine.Conversion.Migration.Commodore;

/// <summary>Valide les noms représentables dans les champs PETSCII Commodore DOS.</summary>
public sealed class CommodoreDosNamePolicy : IMigrationNamePolicy
{
    private readonly FileSystemsCommodoreDosNamePolicy policy = new();

    /// <inheritdoc />
    public bool IsValid(string name) => policy.IsValid(name);
}
