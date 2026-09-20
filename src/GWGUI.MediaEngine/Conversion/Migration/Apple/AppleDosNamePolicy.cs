using GWGUI.MediaEngine.Conversion.Migration;

namespace GWGUI.MediaEngine.Conversion.Migration.Apple;

/// <summary>Valide les noms représentables dans un catalogue Apple DOS.</summary>
public sealed class AppleDosNamePolicy : IMigrationNamePolicy
{
    private readonly GWGUI.MediaFileSystems.FileSystems.Apple.Dos.AppleDosNamePolicy _policy = new();

    /// <inheritdoc />
    public bool IsValid(string name) => _policy.IsValid(name);
}
